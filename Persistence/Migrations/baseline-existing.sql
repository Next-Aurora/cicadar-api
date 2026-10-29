-- 库已经由前端 Prisma 建过表时执行本文件。不需要安装 .NET。
-- 不要对这种库执行 InitialAuthSchema.sql，否则会再次 CREATE TABLE。
-- 本文件只把初始迁移记成已应用，不改现有表。
--   mysql -h127.0.0.1 -u用户 -p 库名 < Persistence/Migrations/baseline-existing.sql

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    PRIMARY KEY (`MigrationId`)
) DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261029135810_InitialAuthSchema', '9.0.9');
