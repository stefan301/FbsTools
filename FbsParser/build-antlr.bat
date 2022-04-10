@echo off
setlocal enableDelayedExpansion

:: Arbeitspfad auf den Pfad des Batchskripts setzen
pushd "%~dp0"

call antlr\antlr4.bat -Dlanguage=CSharp -o generated -package FbsParser -listener FlatBuffers.g4
pause