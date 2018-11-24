-- MySQL dump 10.13  Distrib 8.0.11, for Win64 (x86_64)
--
-- Host: localhost    Database: test
-- ------------------------------------------------------
-- Server version	8.0.11

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
 SET NAMES utf8mb4 ;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `classe`
--

DROP TABLE IF EXISTS `classe`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
 SET character_set_client = utf8mb4 ;
CREATE TABLE `classe` (
  `id` smallint(5) unsigned NOT NULL AUTO_INCREMENT,
  `firstName` varchar(64) NOT NULL,
  `lastName` varchar(64) NOT NULL,
  `age` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=111 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `classe`
--

LOCK TABLES `classe` WRITE;
/*!40000 ALTER TABLE `classe` DISABLE KEYS */;
INSERT INTO `classe` VALUES (1,'Michael','Jackson',80),(7,'Roronoa','Zoro',21),(8,'Luffy','Mugiwara',19),(14,'Barack','Obama',55),(35,'Sophie','Marceau',41),(40,'Naruto','Uzumaki',16),(42,'Inuyashiki','Robot',65),(46,'Sasuke','Uchiwa',45),(51,'Sangoten','DBZ',25),(52,'James','LeBron',33),(58,'Selana','Gomez',19),(62,'alain','Finkielkraut',77),(67,'Alain','Delon',96),(80,'Emma','Stone',28),(84,'Vegeta','DBZ',26),(85,'Emma','Stone',28),(86,'Madara','Uchiwa',45),(90,'Naruto','Uzumaki',18),(91,'Alain','Cocu',66),(92,'Mat','Damon',36),(93,'Janette','Jackson',80),(94,'James','Bond',41),(95,'Goku','DBZ',26),(96,'Sangoten','Shonen',26),(97,'Chichi','DBZ',33),(99,'Gon','Fricks',18),(104,'Sophie','Marceau',44),(108,'Mat','Damon',36);
/*!40000 ALTER TABLE `classe` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2018-11-25  0:33:05
