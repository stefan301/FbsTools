@echo off

SET CLASSPATH=.;antlr-4.9.2-complete.jar;%CLASSPATH%

java org.antlr.v4.Tool %*

