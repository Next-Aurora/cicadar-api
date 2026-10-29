-- 服务器上用 MySQL 客户端执行，不需要安装 .NET。
-- 先连到目标库（库要已存在），例如：
--   mysql -h127.0.0.1 -u用户 -p 库名 < Persistence/Migrations/InitialAuthSchema.sql
-- 已经由 Prisma 建过表的库不要执行本文件，改执行同目录的 baseline-existing.sql。
-- 本文件可重复执行：迁移已登记时会跳过建表。

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE TABLE `user` (
        `id` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `name` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `email` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `emailVerified` tinyint(1) NOT NULL DEFAULT FALSE,
        `image` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `createdAt` datetime(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
        `updatedAt` datetime(3) NOT NULL,
        CONSTRAINT `PK_user` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE TABLE `verification` (
        `id` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `identifier` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `value` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `expiresAt` datetime(3) NOT NULL,
        `createdAt` datetime(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
        `updatedAt` datetime(3) NOT NULL,
        CONSTRAINT `PK_verification` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE TABLE `wechat_login_ticket` (
        `id` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `nonceHash` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `status` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `userId` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `bindUserId` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `expiresAt` datetime(3) NOT NULL,
        `createdAt` datetime(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
        CONSTRAINT `PK_wechat_login_ticket` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE TABLE `account` (
        `id` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `accountId` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `providerId` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `userId` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `accessToken` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `refreshToken` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `idToken` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `accessTokenExpiresAt` datetime(3) NULL,
        `refreshTokenExpiresAt` datetime(3) NULL,
        `scope` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `password` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `createdAt` datetime(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
        `updatedAt` datetime(3) NOT NULL,
        CONSTRAINT `PK_account` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE TABLE `comment` (
        `id` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `userId` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `guideSlug` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `body` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `createdAt` datetime(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
        `updatedAt` datetime(3) NOT NULL,
        CONSTRAINT `PK_comment` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE TABLE `favorite` (
        `id` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `userId` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `guideSlug` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `createdAt` datetime(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
        CONSTRAINT `PK_favorite` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE TABLE `session` (
        `id` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `expiresAt` datetime(3) NOT NULL,
        `token` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        `createdAt` datetime(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
        `updatedAt` datetime(3) NOT NULL,
        `ipAddress` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `userAgent` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
        `userId` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
        CONSTRAINT `PK_session` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE UNIQUE INDEX `account_providerId_accountId_key` ON `account` (`providerId`, `accountId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE INDEX `account_userId_idx` ON `account` (`userId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE INDEX `comment_guideSlug_idx` ON `comment` (`guideSlug`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE INDEX `comment_userId_idx` ON `comment` (`userId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE INDEX `favorite_guideSlug_idx` ON `favorite` (`guideSlug`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE UNIQUE INDEX `favorite_userId_guideSlug_key` ON `favorite` (`userId`, `guideSlug`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE UNIQUE INDEX `session_token_key` ON `session` (`token`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE INDEX `session_userId_idx` ON `session` (`userId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE UNIQUE INDEX `user_email_key` ON `user` (`email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE INDEX `verification_identifier_idx` ON `verification` (`identifier`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    CREATE INDEX `wechat_login_ticket_expiresAt_idx` ON `wechat_login_ticket` (`expiresAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    ALTER TABLE `account` ADD CONSTRAINT `account_userId_fkey` FOREIGN KEY (`userId`) REFERENCES `user` (`id`) ON UPDATE CASCADE ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    ALTER TABLE `comment` ADD CONSTRAINT `comment_userId_fkey` FOREIGN KEY (`userId`) REFERENCES `user` (`id`) ON UPDATE CASCADE ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    ALTER TABLE `favorite` ADD CONSTRAINT `favorite_userId_fkey` FOREIGN KEY (`userId`) REFERENCES `user` (`id`) ON UPDATE CASCADE ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    ALTER TABLE `session` ADD CONSTRAINT `session_userId_fkey` FOREIGN KEY (`userId`) REFERENCES `user` (`id`) ON UPDATE CASCADE ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261029135810_InitialAuthSchema') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261029135810_InitialAuthSchema', '9.0.9');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

