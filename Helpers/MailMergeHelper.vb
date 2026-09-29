Imports TXTextControl
Imports TXTextControl.DocumentServer

Public Module MailMergeHelper

    ' Hier handelt es sich um ein einfaches Beispiel
    ''' Platzhalter mit STRG + F9 an der richtigen Stelle erstellen und MERGEFIELD <Platzhaltername> schreiben
    ''' Dadurch wird funktionales Datenfeld erstellt in Word (.docx)
    ''' im Hintergrund steht dann <w:fldSimple w:instr="MERGEFIELD Name"/>
    ''' Mit F9 sind diese dann im Word Dokument sichtbar
    ''' Info: Aktuell ist das Klasse Person fest verdrahtet!
    Public Sub ProcessMailMerge(person As Person, vorlagepfad As String, sicherungspfad As String)
        Dim settings As New TXTextControl.LoadSettings()
        settings.ApplicationFieldFormat = TXTextControl.ApplicationFieldFormat.MSWord
        settings.ApplicationFieldTypeNames = New String() {"MERGEFIELD"} '

        Using tx As New ServerTextControl()
            tx.Create()

            ' Vorlage laden
            tx.Load(vorlagepfad, StreamType.WordprocessingML, settings)

            ' MailMerge - Komponente mit TextControl verknüpfen
            Using mailMerge As New MailMerge()
                mailMerge.TextComponent = tx
                ' Datenobjekt (z. B. anonymes Object oder Dictonary)
                ' Geben un unserem Beispiel ein Objekt von der Klasse Person mit

                ' Felder automatisch im Hintergrund ausfüllen
                mailMerge.MergeObject(person)
            End Using

            tx.Save(sicherungspfad, StreamType.WordprocessingML)
        End Using
    End Sub

End Module
