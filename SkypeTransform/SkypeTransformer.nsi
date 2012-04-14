; Transformer.nsi
; File di installazione per l'avvio del setup di ClickOnce
; --------------------------------------------------------


Name "Transformer"
SilentInstall silent
OutFile "transformer.exe"

Section "" 

  SetOutPath $TEMP\Transformer
  File /r *
  ExecWait '"$OUTDIR\setup.exe"'
  Delete /REBOOTOK $OUTDIR
  
SectionEnd
