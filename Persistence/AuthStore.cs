using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Cicadar.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Cicadar.Api.Persistence;

public sealed class AuthStoreException : Exception
{
    public AuthStoreException(int status, string message)
        : base(message)
    {
        Status = status;
    }

    public int Status { get; }
}

public sealed class AuthStoreCommand
{
    public string Op { get; set; } = "";

    public string Model { get; set; } = "";

    public List<AuthStoreWhere>? Where { get; set; }

    public Dictionary<string, JsonElement>? Data { get; set; }

    public Dictionary<string, JsonElement>? Update { get; set; }

    public int? Limit { get; set; }

    public int? Offset { get; set; }

    public AuthStoreSort? SortBy { get; set; }
}

public sealed class AuthStoreWhere
{
    public string Field { get; set; } = "";

    public string? Operator { get; set; }

    public JsonElement Value { get; set; }

    public string? Connector { get; set; }
}

public sealed class AuthStoreSort
{
    public string Field { get; set; } = "";

    public string? Direction { get; set; }
}

public sealed class MergeWechatUserCommand
{
    public string WechatUserId { get; set; } = "";

    public string EmailUserId { get; set; } = "";
}

public sealed class AuthStore
{
    public const string WechatProviderId = "wechat-mini";

    private const int MaxLimit = 100;

    private readonly CicadaDbContext db;

    public AuthStore(CicadaDbContext db)
    {
        this.db = db;
    }

    public async Task<object?> Execute(AuthStoreCommand command, CancellationToken cancellationToken)
    {
        var model = AuthModels.Require(command.Model);
        var op = command.Op;
        return op switch
        {
            "create" => await Create(model, command.Data, cancellationToken),
            "findOne" => await Find(model, command, single: true, cancellationToken),
            "findMany" => await Find(model, command, single: false, cancellationToken),
            "count" => await Count(model, command.Where, cancellationToken),
            "update" => await Update(model, command.Where, command.Update, returnRow: true, cancellationToken),
            "updateMany" => await Update(model, command.Where, command.Update, returnRow: false, cancellationToken),
            "delete" => await Delete(model, command.Where, requireCount: false, cancellationToken),
            "deleteMany" => await Delete(model, command.Where, requireCount: true, cancellationToken),
            "consumeOne" => await ConsumeOne(model, command.Where, cancellationToken),
            _ => throw new AuthStoreException(400, "不支持的操作"),
        };
    }

    public async Task MergeWechatUser(MergeWechatUserCommand command, CancellationToken cancellationToken)
    {
        var wechatUserId = command.WechatUserId;
        var emailUserId = command.EmailUserId;
        if (string.IsNullOrWhiteSpace(wechatUserId) || string.IsNullOrWhiteSpace(emailUserId))
        {
            throw new AuthStoreException(400, "缺少要合并的账号");
        }

        if (wechatUserId == emailUserId)
        {
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var wechatAccount = await db.Accounts
            .FirstOrDefaultAsync(
                account => account.UserId == wechatUserId && account.ProviderId == WechatProviderId,
                cancellationToken);
        if (wechatAccount is null)
        {
            throw new AuthStoreException(400, "当前账号没有可绑定的微信");
        }

        var emailAlreadyHasOtherWechat = await db.Accounts.AnyAsync(
            account => account.UserId == emailUserId
                && account.ProviderId == WechatProviderId
                && account.AccountId != wechatAccount.AccountId,
            cancellationToken);
        if (emailAlreadyHasOtherWechat)
        {
            throw new AuthStoreException(400, "该邮箱账号已绑定其他微信");
        }

        var emailFavoriteSlugs = await db.Favorites
            .Where(favorite => favorite.UserId == emailUserId)
            .Select(favorite => favorite.GuideSlug)
            .ToListAsync(cancellationToken);
        var wechatFavorites = await db.Favorites
            .Where(favorite => favorite.UserId == wechatUserId)
            .ToListAsync(cancellationToken);
        foreach (var favorite in wechatFavorites)
        {
            if (emailFavoriteSlugs.Contains(favorite.GuideSlug))
            {
                db.Favorites.Remove(favorite);
            }
            else
            {
                favorite.UserId = emailUserId;
            }
        }

        var comments = await db.Comments.Where(comment => comment.UserId == wechatUserId).ToListAsync(cancellationToken);
        foreach (var comment in comments)
        {
            comment.UserId = emailUserId;
        }

        var tickets = await db.WechatLoginTickets
            .Where(ticket => ticket.UserId == wechatUserId || ticket.BindUserId == wechatUserId)
            .ToListAsync(cancellationToken);
        foreach (var ticket in tickets)
        {
            if (ticket.UserId == wechatUserId)
            {
                ticket.UserId = emailUserId;
            }

            if (ticket.BindUserId == wechatUserId)
            {
                ticket.BindUserId = emailUserId;
            }
        }

        var existingOnEmail = await db.Accounts.FirstOrDefaultAsync(
            account => account.ProviderId == WechatProviderId && account.AccountId == wechatAccount.AccountId,
            cancellationToken);
        if (existingOnEmail is not null && existingOnEmail.UserId == emailUserId)
        {
            db.Accounts.Remove(wechatAccount);
        }
        else
        {
            wechatAccount.UserId = emailUserId;
        }

        var sessions = await db.Sessions.Where(session => session.UserId == wechatUserId).ToListAsync(cancellationToken);
        db.Sessions.RemoveRange(sessions);
        var leftoverAccounts = await db.Accounts.Where(account => account.UserId == wechatUserId).ToListAsync(cancellationToken);
        db.Accounts.RemoveRange(leftoverAccounts);
        var wechatUser = await db.Users.FirstOrDefaultAsync(user => user.Id == wechatUserId, cancellationToken);
        if (wechatUser is not null)
        {
            db.Users.Remove(wechatUser);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<Dictionary<string, object?>?> Create(
        AuthModel model,
        Dictionary<string, JsonElement>? data,
        CancellationToken cancellationToken)
    {
        if (data is null || !data.TryGetValue("id", out var idElement) || idElement.ValueKind != JsonValueKind.String)
        {
            throw new AuthStoreException(400, "缺少 id");
        }

        var entity = Activator.CreateInstance(model.Type)
            ?? throw new AuthStoreException(500, "无法创建记录");
        ApplyValues(model, entity, data);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await db.Entry(entity).ReloadAsync(cancellationToken);
        return AuthModels.ToRow(model, entity);
    }

    private async Task<object?> Find(AuthModel model, AuthStoreCommand command, bool single, CancellationToken cancellationToken)
    {
        var method = typeof(AuthStore).GetMethod(nameof(FindOf), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(model.Type);
        try
        {
            var task = (Task<List<Dictionary<string, object?>>>)method.Invoke(
                this,
                [command.Where, command.Limit, command.Offset, command.SortBy, single, cancellationToken])!;
            var rows = await task;
            return single ? rows.FirstOrDefault() : rows;
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            throw error.InnerException;
        }
    }

    private async Task<List<Dictionary<string, object?>>> FindOf<T>(
        IReadOnlyList<AuthStoreWhere>? where,
        int? limit,
        int? offset,
        AuthStoreSort? sortBy,
        bool single,
        CancellationToken cancellationToken)
        where T : class
    {
        var model = AuthModels.For<T>();
        var query = ApplyWhere(db.Set<T>().AsNoTracking(), model, where);
        query = ApplyOrder(query, model, sortBy);
        var take = single ? 1 : Math.Clamp(limit ?? MaxLimit, 1, MaxLimit);
        var skip = Math.Max(offset ?? 0, 0);
        var rows = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
        return rows.Select(row => AuthModels.ToRow(model, row)).ToList();
    }

    private async Task<int> Count(AuthModel model, IReadOnlyList<AuthStoreWhere>? where, CancellationToken cancellationToken)
    {
        var method = typeof(AuthStore).GetMethod(nameof(CountOf), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(model.Type);
        try
        {
            var task = (Task<int>)method.Invoke(this, [where, cancellationToken])!;
            return await task;
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            throw error.InnerException;
        }
    }

    private Task<int> CountOf<T>(IReadOnlyList<AuthStoreWhere>? where, CancellationToken cancellationToken)
        where T : class
    {
        var model = AuthModels.For<T>();
        return ApplyWhere(db.Set<T>().AsNoTracking(), model, where).CountAsync(cancellationToken);
    }

    private async Task<object?> Update(
        AuthModel model,
        IReadOnlyList<AuthStoreWhere>? where,
        Dictionary<string, JsonElement>? values,
        bool returnRow,
        CancellationToken cancellationToken)
    {
        RequireWhere(where);
        if (values is null || values.Count == 0)
        {
            return returnRow ? null : 0;
        }

        var method = typeof(AuthStore).GetMethod(nameof(UpdateOf), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(model.Type);
        try
        {
            var task = (Task<object?>)method.Invoke(this, [where, values, returnRow, cancellationToken])!;
            return await task;
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            throw error.InnerException;
        }
    }

    private async Task<object?> UpdateOf<T>(
        IReadOnlyList<AuthStoreWhere>? where,
        Dictionary<string, JsonElement> values,
        bool returnRow,
        CancellationToken cancellationToken)
        where T : class
    {
        var model = AuthModels.For<T>();
        var ids = await ApplyWhere(db.Set<T>().AsNoTracking(), model, where)
            .Select(entity => EF.Property<string>(entity, "Id"))
            .ToListAsync(cancellationToken);
        if (ids.Count == 0)
        {
            return returnRow ? null : 0;
        }

        var setters = Expression.Parameter(typeof(SetPropertyCalls<T>), "setters");
        Expression chain = setters;
        foreach (var (field, element) in values)
        {
            var property = model.RequireField(field);
            var entity = Expression.Parameter(typeof(T), "entity");
            var propertyLambda = Expression.Lambda(Expression.Property(entity, property), entity);
            var value = AuthValues.Constant(AuthValues.Read(element, property.PropertyType), property.PropertyType);
            var setProperty = typeof(SetPropertyCalls<T>).GetMethods()
                .Single(method =>
                {
                    if (method.Name != "SetProperty" || !method.IsGenericMethodDefinition)
                    {
                        return false;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length != 2)
                    {
                        return false;
                    }

                    var second = parameters[1].ParameterType;
                    return !(second.IsGenericType && second.GetGenericTypeDefinition() == typeof(Expression<>));
                })
                .MakeGenericMethod(property.PropertyType);
            chain = Expression.Call(chain, setProperty, Expression.Quote(propertyLambda), value);
        }

        var assignment = Expression.Lambda<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>(chain, setters);
        var count = await ApplyWhere(
                db.Set<T>().Where(entity => ids.Contains(EF.Property<string>(entity, "Id"))),
                model,
                where)
            .ExecuteUpdateAsync(assignment, cancellationToken);
        if (!returnRow)
        {
            return count;
        }

        if (count == 0)
        {
            return null;
        }

        var updated = await db.Set<T>().AsNoTracking()
            .FirstOrDefaultAsync(entity => EF.Property<string>(entity, "Id") == ids[0], cancellationToken);
        return updated is null ? null : AuthModels.ToRow(model, updated);
    }

    private async Task<object?> Delete(
        AuthModel model,
        IReadOnlyList<AuthStoreWhere>? where,
        bool requireCount,
        CancellationToken cancellationToken)
    {
        RequireWhere(where);
        var method = typeof(AuthStore).GetMethod(nameof(DeleteOf), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(model.Type);
        try
        {
            var task = (Task<int>)method.Invoke(this, [where, cancellationToken])!;
            var count = await task;
            return requireCount ? count : null;
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            throw error.InnerException;
        }
    }

    private Task<int> DeleteOf<T>(IReadOnlyList<AuthStoreWhere>? where, CancellationToken cancellationToken)
        where T : class
    {
        var model = AuthModels.For<T>();
        return ApplyWhere(db.Set<T>(), model, where).ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<Dictionary<string, object?>?> ConsumeOne(
        AuthModel model,
        IReadOnlyList<AuthStoreWhere>? where,
        CancellationToken cancellationToken)
    {
        RequireWhere(where);
        var method = typeof(AuthStore).GetMethod(nameof(ConsumeOneOf), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(model.Type);
        try
        {
            var task = (Task<Dictionary<string, object?>?>)method.Invoke(this, [where, cancellationToken])!;
            return await task;
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            throw error.InnerException;
        }
    }

    private async Task<Dictionary<string, object?>?> ConsumeOneOf<T>(
        IReadOnlyList<AuthStoreWhere>? where,
        CancellationToken cancellationToken)
        where T : class
    {
        var model = AuthModels.For<T>();
        var row = await ApplyWhere(db.Set<T>().AsNoTracking(), model, where).FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return null;
        }

        var id = model.Fields["id"].GetValue(row) as string;
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var deleted = await db.Set<T>().Where(entity => EF.Property<string>(entity, "Id") == id)
            .ExecuteDeleteAsync(cancellationToken);
        return deleted == 1 ? AuthModels.ToRow(model, row) : null;
    }

    private static IQueryable<T> ApplyWhere<T>(IQueryable<T> query, AuthModel model, IReadOnlyList<AuthStoreWhere>? where)
    {
        if (where is null || where.Count == 0)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "entity");
        var body = AuthWhere.Build(parameter, model, where);
        var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
        return query.Where(predicate);
    }

    private static IQueryable<T> ApplyOrder<T>(IQueryable<T> query, AuthModel model, AuthStoreSort? sortBy)
    {
        if (sortBy is null || string.IsNullOrWhiteSpace(sortBy.Field))
        {
            return query;
        }

        var property = model.RequireField(sortBy.Field);
        var parameter = Expression.Parameter(typeof(T), "entity");
        var key = Expression.Lambda(Expression.Property(parameter, property), parameter);
        var methodName = string.Equals(sortBy.Direction, "desc", StringComparison.OrdinalIgnoreCase)
            ? "OrderByDescending"
            : "OrderBy";
        var method = typeof(Queryable).GetMethods()
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.PropertyType);
        return (IQueryable<T>)method.Invoke(null, [query, key])!;
    }

    private static void ApplyValues(AuthModel model, object entity, Dictionary<string, JsonElement> values)
    {
        foreach (var (field, element) in values)
        {
            var property = model.RequireField(field);
            property.SetValue(entity, AuthValues.Read(element, property.PropertyType));
        }
    }

    private static void RequireWhere(IReadOnlyList<AuthStoreWhere>? where)
    {
        if (where is null || where.Count == 0)
        {
            throw new AuthStoreException(400, "条件不能为空");
        }
    }
}

internal static class AuthWhere
{
    public static Expression Build(ParameterExpression parameter, AuthModel model, IReadOnlyList<AuthStoreWhere> where)
    {
        if (where.Count == 1)
        {
            return Predicate(parameter, model, where[0]);
        }

        var ands = where.Where(item => !string.Equals(item.Connector, "OR", StringComparison.OrdinalIgnoreCase)).ToList();
        var ors = where.Where(item => string.Equals(item.Connector, "OR", StringComparison.OrdinalIgnoreCase)).ToList();
        Expression? combined = ands.Count == 0 ? null : Combine(parameter, model, ands, Expression.AndAlso);
        if (ors.Count > 0)
        {
            var or = Combine(parameter, model, ors, Expression.OrElse);
            combined = combined is null ? or : Expression.AndAlso(combined, or);
        }

        return combined ?? Expression.Constant(true);
    }

    private static Expression Combine(
        ParameterExpression parameter,
        AuthModel model,
        IReadOnlyList<AuthStoreWhere> items,
        Func<Expression, Expression, BinaryExpression> join)
    {
        var result = Predicate(parameter, model, items[0]);
        for (var index = 1; index < items.Count; index++)
        {
            result = join(result, Predicate(parameter, model, items[index]));
        }

        return result;
    }

    private static Expression Predicate(ParameterExpression parameter, AuthModel model, AuthStoreWhere where)
    {
        var property = model.RequireField(where.Field);
        var access = Expression.Property(parameter, property);
        var op = string.IsNullOrWhiteSpace(where.Operator) ? "eq" : where.Operator;
        if (op is "in" or "not_in")
        {
            var contains = Contains(access, property.PropertyType, where.Value);
            return op == "in" ? contains : Expression.Not(contains);
        }

        if (op is "contains" or "starts_with" or "ends_with")
        {
            if (property.PropertyType != typeof(string))
            {
                throw new AuthStoreException(400, "该字段不能做文本匹配");
            }

            var text = AuthValues.Read(where.Value, typeof(string)) as string ?? "";
            var pattern = op switch
            {
                "starts_with" => AuthValues.EscapeLike(text) + "%",
                "ends_with" => "%" + AuthValues.EscapeLike(text),
                _ => "%" + AuthValues.EscapeLike(text) + "%",
            };
            var like = typeof(DbFunctionsExtensions).GetMethod(
                nameof(DbFunctionsExtensions.Like),
                [typeof(DbFunctions), typeof(string), typeof(string)])!;
            return Expression.Call(like, Expression.Property(null, typeof(EF), nameof(EF.Functions)), access, Expression.Constant(pattern));
        }

        var constant = AuthValues.Constant(AuthValues.Read(where.Value, property.PropertyType), property.PropertyType);
        return op switch
        {
            "eq" => Expression.Equal(access, constant),
            "ne" => Expression.NotEqual(access, constant),
            "lt" => Expression.LessThan(access, constant),
            "lte" => Expression.LessThanOrEqual(access, constant),
            "gt" => Expression.GreaterThan(access, constant),
            "gte" => Expression.GreaterThanOrEqual(access, constant),
            _ => throw new AuthStoreException(400, "不支持的比较"),
        };
    }

    private static Expression Contains(Expression access, Type propertyType, JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new AuthStoreException(400, "in 需要数组");
        }

        var underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        var listType = typeof(List<>).MakeGenericType(underlying);
        var list = Activator.CreateInstance(listType)!;
        var add = listType.GetMethod("Add")!;
        foreach (var item in value.EnumerateArray())
        {
            var read = AuthValues.Read(item, underlying);
            if (read is null)
            {
                continue;
            }

            add.Invoke(list, [read]);
        }

        if (value.GetArrayLength() == 0)
        {
            return Expression.Constant(false);
        }

        var contains = listType.GetMethod("Contains", [underlying])!;
        var argument = access.Type == underlying ? access : Expression.Convert(access, underlying);
        return Expression.Call(Expression.Constant(list), contains, argument);
    }
}

internal static class AuthValues
{
    public static object? Read(JsonElement element, Type target)
    {
        var underlying = Nullable.GetUnderlyingType(target) ?? target;
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            if (Nullable.GetUnderlyingType(target) is null && underlying.IsValueType)
            {
                throw new AuthStoreException(400, "字段不能为空");
            }

            return null;
        }

        if (underlying == typeof(string))
        {
            return element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString();
        }

        if (underlying == typeof(bool))
        {
            if (element.ValueKind == JsonValueKind.True) return true;
            if (element.ValueKind == JsonValueKind.False) return false;
            if (element.ValueKind == JsonValueKind.Number) return element.GetInt32() != 0;
            throw new AuthStoreException(400, "布尔值不正确");
        }

        if (underlying == typeof(DateTime))
        {
            if (element.ValueKind != JsonValueKind.String
                || !DateTime.TryParse(
                    element.GetString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var parsed))
            {
                throw new AuthStoreException(400, "时间格式不正确");
            }

            if (parsed.Kind == DateTimeKind.Local)
            {
                parsed = parsed.ToUniversalTime();
            }

            return DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
        }

        if (underlying == typeof(int))
        {
            return element.GetInt32();
        }

        throw new AuthStoreException(400, "字段类型不正确");
    }

    public static Expression Constant(object? value, Type type)
    {
        if (value is null)
        {
            return Expression.Constant(null, type);
        }

        if (value.GetType() == type)
        {
            return Expression.Constant(value, type);
        }

        return Expression.Convert(Expression.Constant(value), type);
    }

    public static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}

internal sealed class AuthModel
{
    public required string Name { get; init; }

    public required string Table { get; init; }

    public required Type Type { get; init; }

    public required IReadOnlyDictionary<string, PropertyInfo> Fields { get; init; }

    public PropertyInfo RequireField(string field)
    {
        if (!Fields.TryGetValue(field, out var property))
        {
            throw new AuthStoreException(400, "未知字段");
        }

        return property;
    }
}

internal static class AuthModels
{
    private static readonly IReadOnlyDictionary<string, AuthModel> Models = Build();

    public static AuthModel Require(string name)
    {
        if (!Models.TryGetValue(name, out var model))
        {
            throw new AuthStoreException(400, "未知数据表");
        }

        return model;
    }

    public static AuthModel For<T>() => Models.Values.Single(model => model.Type == typeof(T));

    public static Dictionary<string, object?> ToRow(AuthModel model, object entity)
    {
        var row = new Dictionary<string, object?>(model.Fields.Count);
        foreach (var (name, property) in model.Fields)
        {
            var value = property.GetValue(entity);
            if (value is DateTime date)
            {
                value = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

            row[name] = value;
        }

        return row;
    }

    private static IReadOnlyDictionary<string, AuthModel> Build()
    {
        return new Dictionary<string, AuthModel>(StringComparer.Ordinal)
        {
            ["user"] = Model("user", "user", typeof(User)),
            ["session"] = Model("session", "session", typeof(Session)),
            ["account"] = Model("account", "account", typeof(Account)),
            ["verification"] = Model("verification", "verification", typeof(Verification)),
            ["wechatLoginTicket"] = Model("wechatLoginTicket", "wechat_login_ticket", typeof(WechatLoginTicket)),
            ["favorite"] = Model("favorite", "favorite", typeof(Favorite)),
            ["comment"] = Model("comment", "comment", typeof(Comment)),
        };
    }

    private static AuthModel Model(string name, string table, Type type)
    {
        var fields = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var underlying = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (underlying != typeof(string) && underlying != typeof(bool) && underlying != typeof(DateTime) && underlying != typeof(int))
            {
                continue;
            }

            fields[char.ToLowerInvariant(property.Name[0]) + property.Name[1..]] = property;
        }

        return new AuthModel
        {
            Name = name,
            Table = table,
            Type = type,
            Fields = fields,
        };
    }
}
