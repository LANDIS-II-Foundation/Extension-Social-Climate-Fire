##This script reads in severity records from nDNBR and summarizes them
#Alec Kretchun PSU 2017

library(foreign)

sev.dat <- read.dbf("I:/LANDIS_code/2017FireExt/FireSeverity/Severity_BA.dbf") 
sev.dat <- sev.dat[(sev.dat$BURNSEV < 10),] #removing burn severity 255 cause i think they're actually NAs
hist(sev.dat$BURNSEV, ylab="Number of fires", xlab="Severity class", main="nDNBR fire severity")

severity.reclassify <- sev.dat$BURNSEV
severity.reclassify[severity.reclassify < 4] <- 1
severity.reclassify[severity.reclassify > 5] <- 3
severity.reclassify[severity.reclassify > 3] <- 2

##Need to attach fire name and remove year from it. I will then attach this to the larger dataframe of all 
## relevant fire variables to extract conditions that produce each intensity

hist(severity.reclassify, ylab="Number of fires", xlab="Binned severity class", main="nDNBR fire severity", 
     ylim=c(0, 2300),labels=TRUE)

sev.names <- gsub("[[:digit:]]","",sev.dat$VB_ID)
sev.names.u <- unique(sev.names)

##Matching fire names from 'FirePerimeters_EXTRACT_REDO.R'

sev.names.u.lower <- tolower(sev.names.u) #switching to lower case
perimeter.names <- tolower(fire.names.manydays)
 #removing 'complex' for more matches

#This doesn't work quite yet, some names don't line up (include 'complex' etc)
fire.name.match <- perimeter.names[perimeter.names %in% sev.names.u.lower] #matching names between databases
