Imports TXTextControl

''' <summary>
''' Zugriff auf benannte Word-Formularfelder (FORMCHECKBOX) nach dem Import.
'''
''' Hintergrund: TX Text Control übernimmt den Feldnamen aus w:ffData/w:name
''' nicht. FormField.Name ist nach dem Import leer, FormField.ID ist bei allen
''' Feldern 0. Ein direktes Gegenstück zu Words FormFields(Name) gibt es nicht.
'''
''' Word legt zu jedem Formularfeld zusätzlich eine Textmarke gleichen Namens
''' an. Die kommt als SubTextPart an - aber nur, wenn beim Laden
''' LoadSettings.LoadSubTextParts = True gesetzt ist (Default ist False).
''' Die Zuordnung Name -> Feld läuft deshalb über die Startposition:
''' Textmarke und Formularfeld beginnen an derselben Stelle.
'''
''' Kein Feldname steht hier fest verdrahtet. Welche Felder es gibt, sagt
''' Checkboxzustaende() zur Laufzeit aus dem geladenen Dokument.
''' </summary>
Public Module FormularHelper

#Region "Öffentliche API"

    ''' <summary>
    ''' Ladeeinstellungen, mit denen benannte Formularfelder ansprechbar bleiben.
    ''' Ohne LoadSubTextParts verwirft der Import die Textmarken und damit die
    ''' einzige verbliebene Spur der Feldnamen.
    ''' </summary>
    Public Function LadeEinstellungen() As LoadSettings

        Dim einstellungen As New LoadSettings()
        einstellungen.LoadSubTextParts = True

        Return einstellungen

    End Function

    ''' <summary>
    ''' Trägt die Feldnamen aus den Textmarken auf die Formularfelder nach.
    ''' Direkt nach dem Laden aufrufen.
    '''
    ''' Danach zeigt auch der Eigenschaften-Dialog von TX den Namen an und
    ''' FormField.Name liefert ihn - beides ist ohne diesen Schritt leer.
    ''' Die Namen bleiben nur in der geladenen Instanz stehen: Beim Speichern
    ''' schreibt TX sie nicht ins DOCX zurück, deshalb muss der Aufruf bei
    ''' jedem Laden erfolgen.
    ''' </summary>
    ''' <param name="tc">Text Control - Objekt</param>
    ''' <returns>Anzahl der benannten Felder.</returns>
    Public Function NamenNachtragen(tc As TextControl) As Integer

        Dim anzahl As Integer = 0

        For Each bereich As SubTextPart In tc.SubTextParts

            For Each feld As FormField In tc.FormFields

                If feld.Start <> bereich.Start Then Continue For

                feld.Name = bereich.Name

                ' Importierte Felder haben alle die ID 0 und sind darüber nicht
                ' unterscheidbar. Eine laufende Nummer macht FormFields.GetItem
                ' benutzbar.
                anzahl += 1
                feld.ID = anzahl

            Next
        Next

        Return anzahl

    End Function

    ''' <summary>
    ''' Alle ansprechbaren Checkboxen des geladenen Dokumentes mit ihrem
    ''' aktuellen Zustand, in Dokumentreihenfolge.
    '''
    ''' Eine leere Liste bedeutet in aller Regel, dass beim Laden die
    ''' Einstellungen aus LadeEinstellungen() gefehlt haben.
    ''' </summary>
    ''' <param name="tc">Text Control - Objekt</param>
    Public Function Checkboxzustaende(tc As TextControl) As List(Of KeyValuePair(Of String, Boolean))

        Dim zustaende As New List(Of KeyValuePair(Of String, Boolean))

        For Each bereich As SubTextPart In tc.SubTextParts

            Dim feld = FindeCheckboxAnPosition(tc, bereich.Start)
            If feld Is Nothing Then Continue For

            zustaende.Add(New KeyValuePair(Of String, Boolean)(bereich.Name, feld.Checked))

        Next

        Return zustaende

    End Function

    ''' <summary>
    ''' Namen aller ansprechbaren Checkboxen - Kurzform von Checkboxzustaende()
    ''' für Meldungen und Diagnose.
    ''' </summary>
    ''' <param name="tc">Text Control - Objekt</param>
    Public Function Checkboxnamen(tc As TextControl) As String()

        Return Checkboxzustaende(tc).
               Select(Function(eintrag) eintrag.Key).
               ToArray()

    End Function

    ''' <summary>
    ''' Setzt eine benannte Checkbox.
    ''' </summary>
    ''' <param name="tc">Text Control - Objekt</param>
    ''' <param name="feldname">Name des Formularfeldes, z. B. "KK_Alkis"</param>
    ''' <param name="aktiviert">Neuer Zustand</param>
    ''' <returns>False, wenn das Dokument kein Feld dieses Namens enthält.</returns>
    Public Function SetzeCheckbox(tc As TextControl, feldname As String, aktiviert As Boolean) As Boolean

        Dim feld = FindeCheckbox(tc, feldname)
        If feld Is Nothing Then Return False

        feld.Checked = aktiviert
        Return True

    End Function

    ''' <summary>
    ''' Setzt mehrere Checkboxen in einem Durchgang. Als werte eignet sich
    ''' alles, was Name/Zustand liefert - auch ein Dictionary oder das
    ''' Ergebnis von Checkboxzustaende().
    ''' </summary>
    ''' <param name="tc">Text Control - Objekt</param>
    ''' <param name="werte">Feldname -> gewünschter Zustand</param>
    ''' <returns>
    ''' Die Namen, zu denen das Dokument kein Feld enthält.
    ''' Leeres Array = alles gesetzt.
    ''' </returns>
    Public Function SetzeCheckboxen(tc As TextControl,
                                    werte As IEnumerable(Of KeyValuePair(Of String, Boolean))) As String()

        Dim unbekannt As New List(Of String)

        For Each eintrag In werte
            If Not SetzeCheckbox(tc, eintrag.Key, eintrag.Value) Then
                unbekannt.Add(eintrag.Key)
            End If
        Next

        Return unbekannt.ToArray()

    End Function

    ''' <summary>
    ''' Liest den Zustand einer benannten Checkbox.
    ''' </summary>
    ''' <param name="tc">Text Control - Objekt</param>
    ''' <param name="feldname">Name des Formularfeldes, z. B. "KK_Alkis"</param>
    ''' <returns>Nothing, wenn das Dokument kein Feld dieses Namens enthält.</returns>
    Public Function LiesCheckbox(tc As TextControl, feldname As String) As Boolean?

        Dim feld = FindeCheckbox(tc, feldname)
        If feld Is Nothing Then Return Nothing

        Return feld.Checked

    End Function

#End Region

#Region "Intern"

    ''' <summary>
    ''' Textmarke des gesuchten Namens suchen und das Formularfeld nehmen, das
    ''' an derselben Position beginnt. Über FormField.ID geht das nicht - die
    ''' ist nach dem Import bei allen Feldern 0.
    ''' </summary>
    Private Function FindeCheckbox(tc As TextControl, feldname As String) As CheckFormField

        ' Nach NamenNachtragen() steht der Name am Feld - dann direkt vergleichen.
        For Each feld As FormField In tc.FormFields
            If TypeOf feld Is CheckFormField AndAlso
               String.Equals(feld.Name, feldname, StringComparison.OrdinalIgnoreCase) Then
                Return DirectCast(feld, CheckFormField)
            End If
        Next

        ' Sonst über die Textmarke, die beim Import den Namen mitgebracht hat.
        For Each bereich As SubTextPart In tc.SubTextParts

            If Not String.Equals(bereich.Name, feldname, StringComparison.OrdinalIgnoreCase) Then Continue For

            Dim feld = FindeCheckboxAnPosition(tc, bereich.Start)
            If feld IsNot Nothing Then Return feld

        Next

        Return Nothing

    End Function

    Private Function FindeCheckboxAnPosition(tc As TextControl, position As Integer) As CheckFormField

        For Each feld As FormField In tc.FormFields
            If TypeOf feld Is CheckFormField AndAlso feld.Start = position Then
                Return DirectCast(feld, CheckFormField)
            End If
        Next

        Return Nothing

    End Function

#End Region

End Module
