/*
SQLyog Community v11.1 (64 bit)
MySQL - 5.6.10-log : Database - mmicore
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`mmicore` /*!40100 DEFAULT CHARACTER SET utf8 */;

USE `mmicore`;

/*Table structure for table `emails` */

DROP TABLE IF EXISTS `emails`;

CREATE TABLE `emails` (
  `ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `key` varchar(30) DEFAULT NULL,
  `Title` varchar(100) DEFAULT NULL,
  `Message` text,
  `Description` text,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `index1` (`key`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

/*Data for the table `emails` */

insert  into `emails`(`ID`,`key`,`Title`,`Message`,`Description`) values (1,'AdminCreateAccount','New Account Creation','&lt;p&gt;Dear #forename# #surname#,&lt;/p&gt;\r\n&lt;p&gt;a new account has been created for you.&lt;/p&gt;\r\n&lt;p&gt;Your login details are as follows:&lt;/p&gt;\r\n&lt;p&gt;username: #username#&lt;/p&gt;\r\n&lt;p&gt;password: #password#&lt;/p&gt;\r\n&lt;p&gt;Kind Regards,&lt;/p&gt;\r\n&lt;p&gt;Chris&lt;/p&gt;','An email which is sent to a user when an admin creates an account for them'),(2,'UserResetPassword','New Password','&lt;p&gt;Dear #forename# #surname#,&lt;/p&gt;\r\n&lt;p&gt;A&amp;nbsp;new password has been requested for your account from #ip# on the #date#.&lt;/p&gt;\r\n&lt;p&gt;Your new password is: #password#&lt;/p&gt;\r\n&lt;p&gt;#sitename# #siteurl#&lt;/p&gt;\r\n&lt;p&gt;Kind Regards,&lt;/p&gt;\r\n&lt;p&gt;&amp;lt;name&amp;gt;&lt;/p&gt;\r\n&lt;p&gt;&amp;nbsp;&lt;/p&gt;','A password generated when a user requests a new password'),(3,'UserRequestUsername','Username Reminder','&lt;p&gt;Dear #forename# #surname#,&lt;/p&gt;\r\n&lt;p&gt;A username reminder was requested from #ip# on #date#.&lt;/p&gt;\r\n&lt;p&gt;Your username is #username#.&lt;/p&gt;\r\n&lt;p&gt;Kind Regards,&lt;/p&gt;\r\n&lt;p&gt;&amp;lt;name&amp;gt;&lt;/p&gt;','An email which is generated when a user forgets their username.');

/*Table structure for table `emailsentlog` */

DROP TABLE IF EXISTS `emailsentlog`;

CREATE TABLE `emailsentlog` (
  `key` varchar(30) DEFAULT NULL,
  `username` varchar(30) DEFAULT NULL,
  `date` datetime DEFAULT NULL,
  UNIQUE KEY `key1` (`key`,`username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `emailsentlog` */

/*Table structure for table `errors` */

DROP TABLE IF EXISTS `errors`;

CREATE TABLE `errors` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `Occurred` datetime NOT NULL,
  `Error` text NOT NULL,
  `Application` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=628 DEFAULT CHARSET=latin1;

/*Data for the table `errors` */

insert  into `errors`(`id`,`Occurred`,`Error`,`Application`) values (626,'2013-02-27 14:54:14','CriticalApplication Error: There was an error accessing your account.','MMI'),(627,'2013-02-27 14:54:14','Domain Error: There was an issue accessing the profile of \'1234\'','AbsenceManager');

/*Table structure for table `groups` */

DROP TABLE IF EXISTS `groups`;

CREATE TABLE `groups` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `Index_2` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;

/*Data for the table `groups` */

insert  into `groups`(`Id`,`Name`) values (2,'Nov_20_1213'),(1,'Year 3');

/*Table structure for table `lockedaccounts` */

DROP TABLE IF EXISTS `lockedaccounts`;

CREATE TABLE `lockedaccounts` (
  `IP` varchar(16) DEFAULT NULL,
  `TimeLocked` datetime DEFAULT NULL,
  UNIQUE KEY `index1` (`IP`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `lockedaccounts` */

/*Table structure for table `log` */

DROP TABLE IF EXISTS `log`;

CREATE TABLE `log` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `Occurred` datetime DEFAULT NULL,
  `Error` text,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

/*Data for the table `log` */

insert  into `log`(`id`,`Occurred`,`Error`) values (1,'2013-09-03 14:24:47','Account Manager: An error occured while fetching the salt for user \'ddf\'');

/*Table structure for table `loginattempts` */

DROP TABLE IF EXISTS `loginattempts`;

CREATE TABLE `loginattempts` (
  `IP` varchar(16) DEFAULT NULL,
  `occurred` datetime DEFAULT NULL,
  KEY `index1` (`IP`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `loginattempts` */

insert  into `loginattempts`(`IP`,`occurred`) values ('147.188.112.124','2013-09-03 15:17:46'),('147.188.112.124','2013-09-03 15:18:00'),('147.188.112.124','2013-09-03 15:18:02'),('147.188.112.124','2013-09-05 11:48:46'),('147.188.112.124','2013-09-05 11:48:53'),('147.188.112.124','2013-09-05 16:43:14'),('147.188.112.124','2013-09-05 16:43:17');

/*Table structure for table `news` */

DROP TABLE IF EXISTS `news`;

CREATE TABLE `news` (
  `id` int(5) NOT NULL AUTO_INCREMENT,
  `Title` varchar(100) DEFAULT NULL,
  `Body` text,
  `AuthorId` int(5) DEFAULT NULL,
  `Created` datetime DEFAULT NULL,
  `Updated` datetime DEFAULT NULL,
  `Image` varchar(250) DEFAULT NULL,
  `Enabled` tinyint(1) DEFAULT '1',
  `Archived` tinyint(1) DEFAULT '0',
  `Attachments` text,
  PRIMARY KEY (`id`),
  KEY `Index1` (`Enabled`,`Archived`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

/*Data for the table `news` */

insert  into `news`(`id`,`Title`,`Body`,`AuthorId`,`Created`,`Updated`,`Image`,`Enabled`,`Archived`,`Attachments`) values (1,'test 1','&lt;p&gt;test page 1&lt;/p&gt;\r\n&lt;p&gt;&amp;nbsp;&lt;/p&gt;\r\n&lt;p&gt;2&lt;/p&gt;',1,'2013-09-12 14:39:09','2013-09-12 14:42:15','2013-09-12 13:20:08.643',1,0,NULL),(2,'This is a second news item!','&lt;p&gt;Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam id mattis sem. Nam aliquam tincidunt accumsan. Etiam ut dapibus velit. In hac habitasse platea dictumst. In eleifend odio nec tincidunt dignissim. Donec lorem nisi, porta non lacus eu, facilisis dapibus lorem. Quisque eget iaculis tortor, euismod ornare tellus. In mauris augue, ullamcorper eu fermentum ac, tincidunt non dolor. Etiam a nibh eu diam tempor rhoncus at in risus. Mauris&lt;/p&gt;',1,'2013-09-06 14:39:05','2013-09-12 13:20:16','2013-09-12 13:20:16.323',1,0,NULL),(3,'This is a third news item!','&lt;p&gt;Need to sort out the order!!&lt;/p&gt;',1,'0001-01-01 00:00:00','2013-09-12 13:07:22','2013-09-12 13:07:22.102',1,0,NULL);

/*Table structure for table `pages` */

DROP TABLE IF EXISTS `pages`;

CREATE TABLE `pages` (
  `id` int(5) NOT NULL AUTO_INCREMENT,
  `Title` varchar(100) DEFAULT NULL,
  `Body` text,
  `Created` datetime DEFAULT NULL,
  `Updated` datetime DEFAULT NULL,
  `Enabled` tinyint(1) DEFAULT '1',
  `Archived` tinyint(1) DEFAULT '0',
  `Attachments` text,
  PRIMARY KEY (`id`),
  KEY `Index1` (`Enabled`,`Archived`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;

/*Data for the table `pages` */

/*Table structure for table `partialpages` */

DROP TABLE IF EXISTS `partialpages`;

CREATE TABLE `partialpages` (
  `ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `key` varchar(30) DEFAULT NULL,
  `Title` varchar(100) DEFAULT NULL,
  `Text` text,
  `Description` text,
  PRIMARY KEY (`ID`),
  KEY `Index1` (`key`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

/*Data for the table `partialpages` */

insert  into `partialpages`(`ID`,`key`,`Title`,`Text`,`Description`) values (1,'Applicant-Home-Header',NULL,'&lt;h1&gt;Welcome to MMI&lt;/h1&gt;\r\n&lt;p&gt;some information and instructions here&lt;/p&gt;','This text appears at the top of the applicant home page'),(2,'Applicant-Home-Footer-Signed',NULL,'&lt;div class=&quot;alert alert-info&quot;&gt;\r\n&lt;h1&gt;What Next?&lt;/h1&gt;\r\n&lt;p&gt;Some information here&lt;/p&gt;\r\n&lt;/div&gt;','This text appears at the bottom of the applicant home page when they haven\'t signed up to an interview'),(3,'Applicant-Home-Footer-Unsigned',NULL,'&lt;p&gt;Please sign up to an interview etc&lt;/p&gt;','This text appears at the bottom of the applicant home page when they have signed up to an interview');

/*Table structure for table `roles` */

DROP TABLE IF EXISTS `roles`;

CREATE TABLE `roles` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(20) DEFAULT NULL,
  `Colour` varchar(10) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;

/*Data for the table `roles` */

insert  into `roles`(`ID`,`Name`,`Colour`) values (1,'Super Admin','4A8FD4'),(2,'Admin','8252B3'),(3,'Moderator','E0B067'),(4,'Applicant','63C763');

/*Table structure for table `signup` */

DROP TABLE IF EXISTS `signup`;

CREATE TABLE `signup` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `Date` datetime NOT NULL,
  `idGroup` int(10) NOT NULL,
  `AcademicYear` varchar(4) NOT NULL,
  `CloseDate` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `Index_2` (`idGroup`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;

/*Data for the table `signup` */

insert  into `signup`(`id`,`Date`,`idGroup`,`AcademicYear`,`CloseDate`) values (1,'2013-11-19 00:00:00',1,'1213','2013-11-19 00:00:00'),(2,'2013-11-20 00:00:00',1,'1213','2013-11-19 00:00:00'),(3,'2013-11-21 00:00:00',2,'1213','2013-11-19 00:00:00'),(4,'2013-11-22 00:00:00',2,'1213','2013-11-19 00:00:00');

/*Table structure for table `slot` */

DROP TABLE IF EXISTS `slot`;

CREATE TABLE `slot` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `Description` varchar(45) NOT NULL,
  `PlacesAvailable` int(10) NOT NULL,
  `Enabled` tinyint(1) NOT NULL DEFAULT '1',
  `idSignUp` int(10) NOT NULL,
  `Time` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `Index_2` (`idSignUp`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=latin1;

/*Data for the table `slot` */

insert  into `slot`(`id`,`Description`,`PlacesAvailable`,`Enabled`,`idSignUp`,`Time`) values (1,'9 am',10,1,1,'0000-00-00 09:00:00'),(2,'10 am',10,1,1,'0000-00-00 10:00:00'),(3,'12 am',20,1,1,'0000-00-00 12:00:00'),(4,'9 am',10,1,2,'0000-00-00 09:00:00'),(5,'10 am',10,1,2,'0000-00-00 10:00:00'),(6,'12 am',20,1,2,'0000-00-00 12:00:00'),(7,'9 am',10,1,3,'0000-00-00 09:00:00'),(8,'10 am',10,1,3,'0000-00-00 10:00:00'),(9,'12 am',20,1,3,'0000-00-00 12:00:00'),(10,'9 am',10,1,4,'0000-00-00 09:00:00'),(11,'10 am',10,1,4,'0000-00-00 10:00:00'),(12,'12 am',20,1,4,'0000-00-00 12:00:00');

/*Table structure for table `userhasgroups` */

DROP TABLE IF EXISTS `userhasgroups`;

CREATE TABLE `userhasgroups` (
  `Id` int(10) NOT NULL AUTO_INCREMENT,
  `IdGroup` int(10) NOT NULL,
  `IdUser` int(10) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `Index_2` (`IdGroup`,`IdUser`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=latin1;

/*Data for the table `userhasgroups` */

insert  into `userhasgroups`(`Id`,`IdGroup`,`IdUser`) values (1,1,1);

/*Table structure for table `userhasroles` */

DROP TABLE IF EXISTS `userhasroles`;

CREATE TABLE `userhasroles` (
  `UserId` int(11) NOT NULL,
  `RoleId` int(11) NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Data for the table `userhasroles` */

insert  into `userhasroles`(`UserId`,`RoleId`) values (1,1);

/*Table structure for table `userhasslots` */

DROP TABLE IF EXISTS `userhasslots`;

CREATE TABLE `userhasslots` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `idSlot` int(10) NOT NULL,
  `idUser` int(10) NOT NULL,
  `SignUpDate` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `Index_2` (`idSlot`,`idUser`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=latin1;

/*Data for the table `userhasslots` */

/*Table structure for table `users` */

DROP TABLE IF EXISTS `users`;

CREATE TABLE `users` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Username` varchar(20) DEFAULT NULL,
  `Forename` varchar(30) DEFAULT NULL,
  `Surname` varchar(30) DEFAULT NULL,
  `Email` varchar(70) DEFAULT NULL,
  `Enabled` tinyint(1) DEFAULT '1',
  `Archived` tinyint(1) DEFAULT '1',
  `Salt` varchar(16) DEFAULT NULL,
  `Password` varchar(40) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `index1` (`Username`,`Email`,`Enabled`,`Archived`,`Salt`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

/*Data for the table `users` */

insert  into `users`(`ID`,`Username`,`Forename`,`Surname`,`Email`,`Enabled`,`Archived`,`Salt`,`Password`) values (1,'withersc','Chris','Withers','chris.withers@gmail.com',1,0,'1af5afda3faa','64CF88636DCD653117FD456B5CE95602');

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
