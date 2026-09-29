Imports TXTextControl
Imports TXTextControl.Windows.Forms.Ribbon
Public Class Form1
    Inherits RibbonForm

    Private isDirty As Boolean = False

    ' Die Datei, in die "Speichern" ohne Rückfrage schreibt. Leer, solange das
    ' Dokument noch keinen Namen hat.
    Private aktuellerPfad As String = ""
    Private aktuellerStreamTyp As StreamType = StreamType.WordprocessingML

    Private bt_UndoTitleBar As RibbonButton = New RibbonButton
    Private bt_RedoTitleBar As RibbonButton = New RibbonButton
    Private bt_SaveTitleBar As RibbonButton = New RibbonButton

    ' Inhalt der Sidebar
    ' Zugriff der Eigenschaften und Events läuft über dieses Feld - nicht über Sidebar2.Content
    Private WithEvents ucFormularfelder As UcFormularfelder

    ' Fenster, in denen die abgedockte Sidebar schon gehangen hat. TX erzeugt
    ' bei jedem Abdocken ein neues - jedes darf nur einmal verdrahtet werden.
    Private sidebarFenster As New List(Of Form)

    ' Beim Programmende darf das Schließen des Sidebar-Fensters nicht mehr
    ' zum Andocken führen.
    Private isFinished As Boolean = False

    ' Shown feuert erneut, wenn das Fenster versteckt und wieder gezeigt wird.
    ' Die Wiederherstellung darf aber nur einmal angeboten werden.
    Private istGestartet As Boolean = False

    ' Sitzung, deren Stand übernommen, aber noch nicht in die eigene
    ' Wiederherstellungsdatei geschrieben wurde. Solange sie hier steht, ist
    ' ihre Datei die einzige Stelle, an der dieser Stand existiert.
    Private uebernommeneKennung As String = ""

    ' Selbsttätiges Speichern ins geöffnete Dokument. Hängt sich selbst an
    ' TextControl1.Changed - der eigene Changed-Handler bleibt unberührt.
    Private WithEvents autoSpeichern As AutoSpeichern

    Sub New()
        ' Aktuelle Assembly des Projektes als EntryAssembly für TX Text Control
        '  festlegen, damit die Lizenzierung korrekt funktioniert.
        TXTextControl.TextControl.EntryAssembly = GetType(Form1).Assembly

        InitializeComponent()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Titelbar erstellen
        AddTitlebarMenu()

        ' Applications Menü erstellen
        AddToApplicationmenu()

        ' Ribbon erstellen
        CreateRibbonGroupAllgemein()
        CreateRibbonGroupHinweis()


        ' Sidebar mit dem eigenen UserControl belegen
        CreateSideBar()

        ' Test mit den Toggle Buttons
        AddToggleButtonMenu()

        ' Anpassen von Ribbons - Meeting vom 16-09-2026
        ' Dim ribbonFontSizeItem As RibbonComboBox = RibbonFormattingTab1.FindItem(RibbonFormattingTab.RibbonItem.TXITEM_FontSize)
        ' ribbonFontSizeItem.Enabled = False

        ' Dim ribbonCopyItem As RibbonButton = RibbonFormattingTab1.FindItem(RibbonFormattingTab.RibbonItem.TXITEM_Copy)
        ' ribbonCopyItem.Text = "Text kopieren!"


    End Sub

    ''' <summary>
    ''' Erst wenn das Fenster steht, wird eine liegengebliebene Sicherung
    ''' angeboten und die selbsttätige Sicherung gestartet.
    '''
    ''' Nicht in Form1_Load: Ein modaler Dialog von dort erscheint vor dem
    ''' ersten Zeichnen, mit grauer Fläche dahinter. Shown kann erneut feuern,
    ''' wenn das Fenster versteckt und wieder gezeigt wird - daher das Flag.
    ''' </summary>
    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown

        If istGestartet Then Exit Sub
        istGestartet = True

        ' Die Sicherung läuft ab hier - ohne Zielpfad und ohne Bedingung, also
        ' auch für ein Dokument, das nie gespeichert wurde.
        '
        ' Und ausdrücklich vor der Wiederherstellung: Deren Laden löst ein
        ' Changed aus, und nur deshalb wird der wiederhergestellte Stand gleich
        ' wieder gesichert. Sonst stünde er nirgends - die Datei, aus der er
        ' kam, ist beim Anbieten bereits verworfen worden.
        autoSpeichern = New AutoSpeichern(TextControl1)

        BieteWiederherstellungAn()

        ' Nur den Pfad setzen, nicht als gesichert abhaken.
        autoSpeichern.Originaldokument = aktuellerPfad

    End Sub

    ''' <summary>
    ''' Eine Sicherung ist misslungen. Kein Dialog - der würde mitten im Tippen
    ''' aufspringen. Der Hinweis wandert in die Statusleiste und bleibt dort
    ''' stehen, bis wieder gesichert werden konnte.
    ''' </summary>
    Private Sub autoSpeichern_Fehlgeschlagen(sender As Object, e As AutoSpeichernFehlerEventArgs) Handles autoSpeichern.Fehlgeschlagen

        lbl_Sicherung.Text = "Sicherung fehlgeschlagen: " & e.Fehler.Message

    End Sub

    ''' <summary>
    ''' Selbsttätig gesichert. Das sagt ausdrücklich nichts über die Datei des
    ''' Anwenders aus - die ist unberührt geblieben. Deshalb bleibt isDirty
    ''' stehen, und aus demselben Grund wird die Undo-Historie nicht angetastet:
    ''' Der Anwender hat nichts angeklickt, ihm darf nichts weggenommen werden.
    ''' </summary>
    Private Sub autoSpeichern_Gesichert(sender As Object, e As AutoSpeichernEventArgs) Handles autoSpeichern.Gesichert

        lbl_Sicherung.Text = "Gesichert " & e.Zeitpunkt.ToLocalTime().ToString("HH:mm:ss")

        UebernahmeAbschliessen()

    End Sub

    ''' <summary>
    ''' Bietet an, was eine abgestürzte Sitzung hinterlassen hat.
    '''
    ''' Jeder Punkt wird genau einmal angeboten und danach in jedem Fall
    ''' verworfen - sonst erschiene er bei jedem Start aufs Neue. Angenommen
    ''' wird höchstens einer: Es gibt nur ein Fenster.
    ''' </summary>
    Private Sub BieteWiederherstellungAn()

        Dim punkte = Wiederherstellung.OffeneWiederherstellungen()
        If punkte.Count = 0 Then Exit Sub

        ' Eigenes Kennzeichen statt einer Abfrage auf uebernommeneKennung: Das
        ' Feld kann zwischendurch wieder leer werden, wenn während des nächsten
        ' Dialoges bereits gesichert wurde. Geladen wird aber nur einmal - es
        ' gibt nur ein Fenster.
        Dim bereitsGeladen As Boolean = False

        For Each punkt In punkte

            If Not bereitsGeladen AndAlso FrageUndStelleWiederHer(punkt) Then

                bereitsGeladen = True

                ' Diesen einen noch nicht wegräumen: Bis die neue Sitzung zum
                ' ersten Mal gesichert hat, ist er die einzige Stelle, an der
                ' der Stand steht. Ein Absturz in dieser Lücke würde ihn sonst
                ' endgültig kosten.
                uebernommeneKennung = punkt.Kennung
                Continue For

            End If

            Wiederherstellung.Verwerfen(punkt.Kennung)

        Next

    End Sub

    ''' <summary>
    ''' Der übernommene Stand ist jetzt in der Wiederherstellungsdatei dieser
    ''' Sitzung angekommen. Der alte Punkt hat seinen Zweck erfüllt.
    ''' </summary>
    Private Sub UebernahmeAbschliessen()

        If uebernommeneKennung = "" Then Exit Sub

        Wiederherstellung.Verwerfen(uebernommeneKennung)
        uebernommeneKennung = ""

    End Sub

    ''' <summary>
    ''' Fragt zu einem Punkt nach und stellt ihn bei Zustimmung her.
    ''' </summary>
    ''' <returns>True, wenn das Dokument geladen wurde.</returns>
    Private Function FrageUndStelleWiederHer(punkt As Wiederherstellungspunkt) As Boolean

        Dim antwort = MessageBox.Show(
            "Die Anwendung wurde beim letzten Mal nicht ordnungsgemäß beendet." & vbCrLf & vbCrLf &
            "Dokument: " & punkt.Anzeigename & vbCrLf &
            "Stand vom: " & punkt.Gesichert.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss") & vbCrLf & vbCrLf &
            "Möchten Sie diesen Stand wiederherstellen?",
            "Wiederherstellung",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

        If antwort <> DialogResult.Yes Then Return False

        Try
            ' Dieselbe Reihenfolge wie beim Öffnen einer Datei: LadeEinstellungen
            ' bringt die Textmarken mit, NamenNachtragen überträgt die Namen auf
            ' die Formularfelder.
            TextControl1.Load(IO.File.ReadAllBytes(punkt.Datendatei),
                              BinaryStreamType.InternalUnicodeFormat,
                              FormularHelper.LadeEinstellungen())

            FormularHelper.NamenNachtragen(TextControl1)
            ZeigeFormularfelderInSidebar()

        Catch ex As Exception
            MessageBox.Show(
                "Der Stand konnte nicht wiederhergestellt werden:" & vbCrLf & ex.Message,
                "Wiederherstellung fehlgeschlagen",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

            Return False
        End Try

        ' Der Pfad des Originals wird übernommen, damit "Speichern" weiss, wohin.
        aktuellerPfad = punkt.Originalpfad
        aktuellerStreamTyp = GetStreamTypeFromExtension(punkt.Originalpfad)

        TextControl1.ClearUndo()

        ' Der wiederhergestellte Stand steht so nicht in der Originaldatei -
        ' das Dokument ist schmutzig, und das muss es auch bleiben.
        isDirty = True
        AktualisiereTitelleiste()

        Return True

    End Function


#Region "UI"

    Private Sub AddTitlebarMenu()

        bt_SaveTitleBar = RibbonHelper.CreateRibbonButtonSmallNonLabeld("TXITEM_Save", DeviceDpi, AddressOf bt_Save_Click)
        bt_UndoTitleBar = RibbonHelper.CreateRibbonButtonSmallNonLabeld("TXITEM_Undo", DeviceDpi, AddressOf Undo_Click)
        bt_RedoTitleBar = RibbonHelper.CreateRibbonButtonSmallNonLabeld("TXITEM_Redo", DeviceDpi, AddressOf Redo_Click)

        bt_SaveTitleBar.Enabled = False
        bt_UndoTitleBar.Enabled = False
        bt_RedoTitleBar.Enabled = False

        Dim customItems() As RibbonButton = {bt_UndoTitleBar, bt_RedoTitleBar, bt_SaveTitleBar}
        SetQuickAccessToolbarStandardItems(customItems)
    End Sub

    ' Frage 1
    Private Sub AddToApplicationmenu()

        ' Anpassen von Steuerelementen die im Editor hinzugefügt wurden
        bt_Open.LargeIcon = ImageHelper.GetTextControlLargeIcon("TXITEM_Open", DeviceDpi)
        bt_Open.SmallIcon = ImageHelper.GetTextControlSmallIcon("TXITEM_Open", DeviceDpi)

        bt_Save.LargeIcon = ImageHelper.GetTextControlLargeIcon("TXITEM_Save", DeviceDpi)
        bt_Save.SmallIcon = ImageHelper.GetTextControlSmallIcon("TXITEM_Save", DeviceDpi)


        ' Steuerelemente hinzufügen über den Code
        Dim bt_New = RibbonHelper.CreateRibbonButtonLarge("Neu", "TXITEM_New", DeviceDpi, AddressOf ApplicationMenuNew_Click)

        ' Steuerelement an eine direkte Position setzen
        ' Es gitb auch die Funktion .Add() - fügt ein Element ganz ans Ende ein
        Ribbon1.ApplicationMenuItems.Insert(0, bt_New)


        Dim menuDivider As New RibbonSeperator()
        Ribbon1.ApplicationMenuItems.Add(menuDivider)


        Dim bt_PrintMenu As New RibbonMenuButton() With
        {
            .Text = "Drucken ...",
            .DisplayMode = IconTextRelation.LargeIconLabeled,
            .LargeIcon = ImageHelper.GetTextControlLargeIcon("TXITEM_Print", DeviceDpi)
        }

        ' Steuerelemente hinzufügen über den Code
        Dim bt_Print = RibbonHelper.CreateRibbonButtonSmall("Drucken...", "TXITEM_Print", DeviceDpi, AddressOf ApplicationMenuPrint_Click)
        Dim bt_Fastprint = RibbonHelper.CreateRibbonButtonSmall("Schnelldrucken", "TXITEM_PrintQuick", DeviceDpi, AddressOf ApplicationMenuPrint_Click)
        Dim bt_PrintPreview = RibbonHelper.CreateRibbonButtonSmall("Druckvorschau...", "TXITEM_PrintQuick", DeviceDpi, AddressOf ApplicationMenuPrint_Click)

        ' Steuerelemente zum Menu hinzufügen
        bt_PrintMenu.DropDownItems.Add(bt_Print)
        bt_PrintMenu.DropDownItems.Add(bt_Fastprint)
        bt_PrintMenu.DropDownItems.Add(bt_PrintPreview)

        Ribbon1.ApplicationMenuItems.Add(bt_PrintMenu)

    End Sub

    Private Sub CreateRibbonGroupAllgemein()
        ' RibbonGroup erstellen
        Dim ribbonGroupAllgemein As New RibbonGroup()
        ribbonGroupAllgemein.Text = "Allgemein"

        ' Buttons erstellen
        Dim bt_ImportPlatzhalter = CreateRibbonButtonLarge("Platzhalter ersetzen", "TXITEM_Save", DeviceDpi, AddressOf bt_Ersetzen_Click)
        Dim bt_ErsetzenPlatzhalter = CreateRibbonButtonLarge("Platzhalter einfügen", "", DeviceDpi, AddressOf bt_Ersetzen_Click)
        Dim bt_Window = RibbonHelper.CreateRibbonButtonLarge("Eingabe", "input.png", DeviceDpi, AddressOf Input_Click)

        Dim bt_ScrollTo = RibbonHelper.CreateRibbonButtonLarge("Scrollen", "TXITEM_SHAPE_UpArrow", DeviceDpi, AddressOf ScrollTo_Click)
        Dim bt_MailMerge = RibbonHelper.CreateRibbonButtonSmall("Dokument erstellen", "TXITEM_Plus", DeviceDpi, AddressOf MailMerge_Click)
        Dim bt_SetImage = RibbonHelper.CreateRibbonButtonSmall("Bild (Datei)", "TXITEM_InsertImage", DeviceDpi, AddressOf SetImage_Click)
        Dim bt_SetImageWithPosition = RibbonHelper.CreateRibbonButtonSmall("Bild (Datei + Position)", "TXITEM_InsertImage", DeviceDpi, AddressOf SetImageWithPoints_Click)
        Dim bt_Sidebar = RibbonHelper.CreateRibbonButtonSmall("Sidebar anzeigen", "TXITEM_Comments_Sidebars", DeviceDpi, AddressOf bt_Sidebar_Click)


        ' Button zur RibbonGroup hinzufügen
        ribbonGroupAllgemein.RibbonItems.Add(bt_ErsetzenPlatzhalter)
        ribbonGroupAllgemein.RibbonItems.Add(bt_ImportPlatzhalter)
        ribbonGroupAllgemein.RibbonItems.Add(bt_ScrollTo)
        ribbonGroupAllgemein.RibbonItems.Add(bt_Window)
        ribbonGroupAllgemein.RibbonItems.Add(bt_MailMerge)
        ribbonGroupAllgemein.RibbonItems.Add(bt_SetImage)
        ribbonGroupAllgemein.RibbonItems.Add(bt_SetImageWithPosition)
        ribbonGroupAllgemein.RibbonItems.Add(bt_Sidebar)

        ' RibbonGroup zur RibbonTab hinzufügen
        RibbonTab1.RibbonGroups.Add(ribbonGroupAllgemein)
    End Sub

    Private Sub CreateRibbonGroupHinweis()
        ' [Hinweis] RibbonGroup erstellen
        Dim ribbonGroupHint As New RibbonGroup()
        ribbonGroupHint.Text = "Hinweise"

        ' [Hinweis] Dropdown erstellen
        Dim dropdownButton = New RibbonMenuButton() With
        {
            .Text = "Hinweis",
            .DisplayMode = IconTextRelation.LargeIconLabeled,
            .LargeIcon = ImageHelper.GetTextControlLargeIcon("TXITEM_PrintLayout", DeviceDpi)
        }

        ' [Hinweis] Buttons erstellen
        Dim bt_hinweisStarkregen = RibbonHelper.CreateRibbonButtonSmall("Starkregen", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintStarkregen_Click)
        Dim bt_hinweisAbweichung = RibbonHelper.CreateRibbonButtonSmall("Abweichung", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddAbweichung_Click)
        Dim bt_hinweisArchaelogie = RibbonHelper.CreateRibbonButtonSmall("Archaeologie", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddArchaeologie_Click)
        Dim bt_hinweisTrennung = RibbonHelper.CreateRibbonButtonSmall("Trennung", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddTrennung_Click)
        Dim bt_hinweisDichtallgemein = RibbonHelper.CreateRibbonButtonSmall("Dicht allgemein", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintDICHTALLG_Click)
        Dim bt_hinweisDichtJahr = RibbonHelper.CreateRibbonButtonSmall("Dicht Jahr", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintDICHTJAHR_Click)
        Dim bt_hinweisDichtUmbau = RibbonHelper.CreateRibbonButtonSmall("Dicht Umbau", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintDICHTUMBAU_Click)
        Dim bt_hinweisDichtDSGVO = RibbonHelper.CreateRibbonButtonSmall("Dicht DSGVO", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintDSGCO_Click)
        Dim bt_hinweisGrundbuch = RibbonHelper.CreateRibbonButtonSmall("Grundbuch", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintGRUNDBUCH_Click)
        Dim bt_hinweisKMR = RibbonHelper.CreateRibbonButtonSmall("KMR", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintKMR_Click)
        Dim bt_hinweisKontaktBauleiter = RibbonHelper.CreateRibbonButtonSmall("Kontakt Bauleiter", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintKONTAKTBAULEITER_Click)
        Dim bt_hinweisSperrwasser = RibbonHelper.CreateRibbonButtonSmall("Sperrwasser", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintSperrwasser_Click)
        Dim bt_hinweisZusABN = RibbonHelper.CreateRibbonButtonSmall("Zus ABN", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintZUS_ABN_Click)

        ' [Hinweis] Buttons zu Dropdown hinzfügen
        dropdownButton.DropDownItems.Add(bt_hinweisAbweichung)
        dropdownButton.DropDownItems.Add(bt_hinweisArchaelogie)
        dropdownButton.DropDownItems.Add(bt_hinweisDichtallgemein)
        dropdownButton.DropDownItems.Add(bt_hinweisDichtJahr)
        dropdownButton.DropDownItems.Add(bt_hinweisDichtUmbau)
        dropdownButton.DropDownItems.Add(bt_hinweisDichtDSGVO)
        dropdownButton.DropDownItems.Add(bt_hinweisGrundbuch)
        dropdownButton.DropDownItems.Add(bt_hinweisKMR)
        dropdownButton.DropDownItems.Add(bt_hinweisKontaktBauleiter)
        dropdownButton.DropDownItems.Add(bt_hinweisStarkregen)
        dropdownButton.DropDownItems.Add(bt_hinweisTrennung)
        dropdownButton.DropDownItems.Add(bt_hinweisSperrwasser)
        dropdownButton.DropDownItems.Add(bt_hinweisZusABN)

        ' [Hinweis] Dropdown zu Gruppe hinzufügen
        ribbonGroupHint.RibbonItems.Add(dropdownButton)


        ' [Hinweis (nummerisch)] Dropdown erstellen
        Dim dropdownButtonNumm = New RibbonMenuButton() With
        {
            .Text = "Hinweise (nummerisch)",
            .DisplayMode = IconTextRelation.LargeIconLabeled,
            .LargeIcon = ImageHelper.GetTextControlLargeIcon("TXITEM_PrintLayout", DeviceDpi)
        }

        ' [Hinweis (nummerisch)] Buttons erstellen
        Dim bt_hinweisNummerischStarkregen = RibbonHelper.CreateRibbonButtonSmall("Starkregen", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintStarkregenNum_Click)
        Dim bt_hinweisNummerischAbweichung = RibbonHelper.CreateRibbonButtonSmall("Abweichung", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddAbweichungNum_Click)
        Dim bt_hinweisNummerischArchaelogie = RibbonHelper.CreateRibbonButtonSmall("Archaeologie", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddArchaeologieNum_Click)
        Dim bt_hinweisNummerischTrennung = RibbonHelper.CreateRibbonButtonSmall("Trennung", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddTrennungNum_Click)
        Dim bt_hinweisNummerischDichtAllgemein = RibbonHelper.CreateRibbonButtonSmall("Dicht allgemein", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintDICHTALLGNUM_Click)
        Dim bt_hinweisNummerischDichtJahr = RibbonHelper.CreateRibbonButtonSmall("Dicht Jahr", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintDICHTJAHRNUM_Click)
        Dim bt_hinweisNummerischDichtUmbau = RibbonHelper.CreateRibbonButtonSmall("Dicht Umbau", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintDICHTUMBAUNUM_Click)
        Dim bt_hinweisNummerischDSGVO = RibbonHelper.CreateRibbonButtonSmall("Dicht DSGVO", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintDSGCONUM_Click)
        Dim bt_hinweisNummerischGrundbuch = RibbonHelper.CreateRibbonButtonSmall("Grundbuch", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintGRUNDBUCHNUM_Click)
        Dim bt_hinweisNummerischKMR = RibbonHelper.CreateRibbonButtonSmall("KMR", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintKMRNUM_Click)
        Dim bt_hinweisNummerischKontaktBauleiter = RibbonHelper.CreateRibbonButtonSmall("Kontakt Bauleiter", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintKONTAKTBAULEITERNUM_Click)
        Dim bt_hinweisNummerischSperrwasser = RibbonHelper.CreateRibbonButtonSmall("Sperrwasser", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintSperrwasserNUM_Click)
        Dim bt_hinweisNummerischZusABN = RibbonHelper.CreateRibbonButtonSmall("Zus ABN", "TXITEM_PrintLayout", DeviceDpi, AddressOf AddHintZUS_ABNNUM_Click)


        ' [Hinweis (nummerisch)] Buttons zu Dropdown hinzfügen
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischAbweichung)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischArchaelogie)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischTrennung)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischDichtAllgemein)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischDichtJahr)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischDichtUmbau)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischGrundbuch)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischDSGVO)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischKMR)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischKontaktBauleiter)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischStarkregen)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischSperrwasser)
        dropdownButtonNumm.DropDownItems.Add(bt_hinweisNummerischZusABN)

        ' [Hinweis (nummerisch)] Dropdown zu Gruppe hinzufügen
        ribbonGroupHint.RibbonItems.Add(dropdownButtonNumm)

        ' RibbonGroup zur RibbonTab hinzufügen
        RibbonTab1.RibbonGroups.Add(ribbonGroupHint)
    End Sub


    Private Sub AddToggleButtonMenu()

        Dim toggleGroup As New RibbonGroup() With
        {
            .Text = "ToggleButton"
        }

        Dim toggle = CreateToggleButtonWithDefaultIcon("", "TXITEM_Add", DeviceDpi, IconTextRelation.SmallIconLabeled)
        Dim toggleOnOff = CreateToggleButtonWithDefaultChangeIcon("", "", "TXITEM_Unchecked", "TXITEM_Checked", DeviceDpi, IconTextRelation.SmallIconLabeled)

        Dim toggleNothing = CreateToggleButtonWithDefaultIcon("", "", DeviceDpi, IconTextRelation.SmallIconLabeled)
        Dim toggleDefaultIcon = CreateToggleButtonWithDefaultIcon("Toggle mit Default-Icon", "", DeviceDpi, IconTextRelation.SmallIconLabeled)


        toggleGroup.RibbonItems.Add(toggle)
        toggleGroup.RibbonItems.Add(toggleOnOff)
        toggleGroup.RibbonItems.Add(toggleNothing)
        toggleGroup.RibbonItems.Add(toggleDefaultIcon)

        RibbonTab1.RibbonGroups.Add(toggleGroup)
    End Sub
#Region "Klick-Events"

    Private Sub AddHintZUS_ABNNUM_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("ZUS_ABN.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddHintSperrwasserNUM_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("SPERRWASSER.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddHintKONTAKTBAULEITERNUM_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("KONTAKT_BAULEITER.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddHintKMRNUM_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("KMR.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddHintGRUNDBUCHNUM_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("GRUNDBUCH.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddHintDICHTUMBAUNUM_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("DICHT_UMABU.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddHintDSGCONUM_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("DSGVO.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddHintDICHTALLGNUM_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("DICHT_ALLG.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddHintDICHTJAHRNUM_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("DICHT_JAHR.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddHintZUS_ABN_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("ZUS_ABN.docx", "Hinweis")
    End Sub

    Private Sub AddHintSperrwasser_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("SPERRWASSER.docx", "Hinweis")
    End Sub

    Private Sub AddHintKONTAKTBAULEITER_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("KONTAKT_BAULEITER.docx", "Hinweis")
    End Sub

    Private Sub AddHintKMR_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("KMR.docx", "Hinweis")
    End Sub

    Private Sub AddHintGRUNDBUCH_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("GRUNDBUCH.docx", "Hinweis")
    End Sub

    Private Sub AddHintDICHTUMBAU_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("DICHT_UMBAU.docx", "Hinweis")
    End Sub

    Private Sub AddHintDSGCO_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("DSGVO.docx", "Hinweis")
    End Sub

    Private Sub AddHintDICHTALLG_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("DICHT_ALLG.docx", "Hinweis")
    End Sub

    Private Sub AddHintDICHTJAHR_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("DICHT_JAHR.docx", "Hinweis")
    End Sub

    Private Sub AddHintStarkregen_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("STARKREGEN.docx", "Hinweis")
    End Sub

    Private Sub AddAbweichung_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("ABWEICHUNG.docx", "Hinweis")
    End Sub

    Private Sub AddArchaeologie_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("ARCHAEOLOGIE.docx", "Hinweis")
    End Sub

    Private Sub AddTrennung_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("TRENNUNG.docx", "Hinweis")
    End Sub

    Private Sub AddHintStarkregenNum_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("STARKREGEN.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddAbweichungNum_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("ABWEICHUNG.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddArchaeologieNum_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("ARCHAEOLOGIE.docx", "HinweisNummerisch")
    End Sub

    Private Sub AddTrennungNum_Click(sender As Object, e As EventArgs)
        FuegeBausteinEin("TRENNUNG.docx", "HinweisNummerisch")
    End Sub

    Private Sub ScrollTo_Click(sender As Object, e As EventArgs)

        ' Variante 1: Funktioniert
        'TextControl1.ScrollLocation = New System.Drawing.Point(0, 0)

        ' Funktioniert nicht, gibt nothing zurück
        '        TextControl1.ViewMode = TXTextControl.ViewMode.PageView
        '       Application.DoEvents()
        '      Dim ersteSeite = TextControl1.GetPages()(0)
        '     TextControl1.ScrollLocation = New Point(TextControl1.ScrollLocation.X, ersteSeite.Bounds.Top)


        ' Variante 2. Cursor an den Anfang des Dokuments setzen (Position 0)
        ' Text Control scrollt dann automatisch zum Cursor
        TextControl1.Selection.Start = 0
        TextControl1.Selection.Length = 0

    End Sub

    Private Sub Input_Click(sender As Object, e As EventArgs)
        ' 1. Instanz des extra Fensters erstellen
        Dim extraForm As New Form2()

        ' 2. Fenster modal öffnen und auf das Schließen warten
        If extraForm.ShowDialog() = DialogResult.OK Then

            ' 3. Daten aus der Eigenschaft auslesen
            Dim ergebnis As String = extraForm.RueckgabeText

            ' 4. Text an der aktuellen Cursorposition im TX Text Control einfügen
            TextControl1.Selection.Text = ergebnis

            MessageBox.Show("Daten erfolgreich in TX Text Control eingefügt!")
        End If

        ' 5. Ressourcen des Fensters freigeben
        extraForm.Dispose()
    End Sub

    Private Sub MailMerge_Click(sender As Object, e As EventArgs)
        Dim person As New Person() With {
            .Vorname = "Max",
            .Nachname = "Mustermann",
            .Strasse = "Musterstraße 1",
            .Plz = "12345",
            .Ort = "Musterstadt"
        }

        Dim vorlage As String = "" ' -> Pfad + Dateiname für die Vorlage hinzufügen
        Dim sicherung As String = "" '-> Pfad + Dateinamen für die Sicherung hinzufgen

        MailMergeHelper.ProcessMailMerge(person, vorlage, sicherung)

        MessageBox.Show("Dokument wurde im Hintergrund erstellt", "Dokument fertiggestellt!")
    End Sub

    Private Sub SetImage_Click(sender As Object, e As EventArgs)
        ImageHelper.LoadImageFromFile("Images\flower.jpg", TextControl1)
    End Sub

    Private Sub SetImageWithPoints_Click(sender As Object, e As EventArgs)
        ImageHelper.LoadImageFromFile("Images\flower.jpg", TextControl1, New Point(500, 500))
    End Sub


    Private Sub bt_Ersetzen_Click(sender As Object, e As EventArgs)
        Dim person As New Person() With {
            .Vorname = "Max",
            .Nachname = "Mustermann",
            .Strasse = "Musterstraße 1",
            .Plz = "12345",
            .Ort = "Musterstadt"
        }

        ErsetzePlatzhalter("{Vorname}", person.Vorname, -1)
        ErsetzePlatzhalter("{Nachname}", person.Nachname, -1)
        ErsetzePlatzhalter("{Strasse}", person.Strasse, -1)
        ErsetzePlatzhalter("{Plz}", person.Plz, -1)
        ErsetzePlatzhalter("{Ort}", person.Ort, -1)
    End Sub

    Private Sub ErsetzePlatzhalter(platzhalter As String, ersatzwert As String, position As Integer)
        ' Platzhalter im Dokument suchen und ersetzen
        Do
            position = TextControl1.Find(platzhalter, position + 1,
                TXTextControl.FindOptions.NoHighlight Or TXTextControl.FindOptions.NoMessageBox)

            If position < 0 Then
                Exit Do
            End If

            ' Platzhalter ersetzen
            TextControl1.Selection.Start = position
            TextControl1.Selection.Length = platzhalter.Length
            TextControl1.Selection.Text = ersatzwert

            position += ersatzwert.Length ' Position nach dem ersetzten Text setzen
        Loop
    End Sub


    Private Sub ApplicationMenuNew_Click(sender As Object, e As EventArgs)
        MessageBox.Show("Diese Funktion ist aktuell nicht implementiert", "Nicht vorhanden!")
    End Sub

    Private Sub ApplicationMenuPrint_Click(sender As Object, e As EventArgs)
        MessageBox.Show("Diese Funktion ist aktuell nicht implementiert", "Nicht vorhanden!")
    End Sub


    Private Sub Undo_Click(sender As Object, e As EventArgs)
        If TextControl1.CanUndo Then
            TextControl1.Undo()
        End If
    End Sub

    Private Sub Redo_Click(sender As Object, e As EventArgs)
        If TextControl1.CanRedo Then
            TextControl1.Redo()
        End If
    End Sub

#End Region
#End Region

    ''' _________ Sontiges _________

    Private Sub TextControl1_InputPositionChanged(sender As Object, e As EventArgs) Handles TextControl1.InputPositionChanged
        KontextTabWerkz.Visible = TextControl1.Tables.GetItem() IsNot Nothing
        If KontextTabWerkz.Visible Then
            Ribbon1.SelectedTab = Ribbon1.TabPages(Ribbon1.TabPages.Count - 1)
        End If
    End Sub

    Private Sub TextControl1_DrawingSelected(sender As Object, e As TXTextControl.DataVisualization.DrawingEventArgs) Handles TextControl1.DrawingSelected
        KontextRahmenWerkz.Visible = True
        Ribbon1.SelectedTab = Ribbon1.TabPages(Ribbon1.TabPages.Count - 1)
    End Sub

    Private Sub TextControl1_DrawingDeselected(sender As Object, e As TXTextControl.DataVisualization.DrawingEventArgs) Handles TextControl1.DrawingDeselected
        KontextRahmenWerkz.Visible = False
    End Sub

    Private Sub TextControl1_FrameSelected(sender As Object, e As TXTextControl.FrameEventArgs) Handles TextControl1.FrameSelected
        KontextRahmenWerkz.Visible = True
        Ribbon1.SelectedTab = Ribbon1.TabPages(Ribbon1.TabPages.Count - 1)
    End Sub

    Private Sub TextControl1_FrameDeselected(sender As Object, e As TXTextControl.FrameEventArgs) Handles TextControl1.FrameDeselected
        KontextRahmenWerkz.Visible = False
    End Sub

    Private Sub bt_ImportPlatzhalter_Click(sender As Object, e As EventArgs)

        FuegeBausteinEin("Baustein 1.docx", "Bausteine")
        FuegeBausteinEin("Baustein 2.docx", "Bausteine")
        FuegeBausteinEin("Baustein 3.docx", "Bausteine")
    End Sub


    ''' <summary>
    ''' Öffnen eines Dokumentes
    ''' Filter kann man erweitern <see cref="StreamType"/>
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub bt_Open_Click(sender As Object, e As EventArgs) Handles bt_Open.Click

        ' Ungespeicherte Änderungen abfragen, bevor das Dokument ersetzt wird.
        '
        ' Mit Speichern-Möglichkeit, nicht nur "trotzdem öffnen": Die
        ' selbsttätige Sicherung fasst die Datei des Anwenders nicht mehr an,
        ' und sie stellt ab dem nächsten Takt auf das neue Dokument um. Wer
        ' hier ohne zu speichern weitergeht, verliert seine Änderungen endgültig.
        If isDirty Then

            Dim antwort = MessageBox.Show(
                "Das aktuelle Dokument enthält ungespeicherte Änderungen." & vbCrLf & vbCrLf &
                "Möchten Sie sie speichern, bevor ein anderes Dokument geöffnet wird?",
                "Ungespeicherte Änderungen",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning)

            If antwort = DialogResult.Cancel Then Exit Sub

            If antwort = DialogResult.Yes AndAlso Not SpeichereAktuelles() Then Exit Sub

        End If

        Dim openDlg As New OpenFileDialog()

        ' Filter setzen, wenn gewünscht
        openDlg.Filter = "Alle unterstützten Dateien|*.docx;*.doc;*.rtf;*.html;*.htm;*.txt;*.tx|" &
                 "Word-Dokumente (*.docx)|*.docx|" &
                 "Rich Text Format (*.rtf)|*.rtf|" &
                 "HTML-Dateien (*.html)|*.html;*.htm|" &
                 "Reiner Text (*.txt)|*.txt|" &
                 "Text Control (*.tx)|*.tx"

        ' Dialog öffnen
        If openDlg.ShowDialog() <> DialogResult.OK Then Exit Sub

        Dim streamTyp As StreamType

        Select Case openDlg.FilterIndex
            Case 2 ' .docx
                streamTyp = TXTextControl.StreamType.WordprocessingML
            Case 3 ' .rtf
                streamTyp = TXTextControl.StreamType.RichTextFormat
            Case 4 ' .html
                streamTyp = TXTextControl.StreamType.HTMLFormat
            Case 5 ' .txt
                streamTyp = TXTextControl.StreamType.PlainText
            Case 6 ' .tx
                streamTyp = TXTextControl.StreamType.InternalUnicodeFormat
            Case Else
                ' "Alle unterstützten Dateien": Format anhand der Dateiendung bestimmen
                streamTyp = GetStreamTypeFromExtension(openDlg.FileName)
        End Select

        Try
            ' Lädt die Datei direkt in das TextControl (ersetzt den bisherigen Inhalt).
            ' LadeEinstellungen() bringt die Textmarken mit - ohne sie sind benannte
            ' Formularfelder (Checkboxen) hinterher nicht mehr ansprechbar.
            TextControl1.Load(openDlg.FileName, streamTyp, FormularHelper.LadeEinstellungen())

            ' Feldnamen aus den Textmarken auf die Formularfelder übertragen.
            ' TX liest den Namen beim Import nicht mit - ohne diesen Schritt
            ' bleibt FormField.Name leer, auch im Eigenschaften-Dialog von TX.
            FormularHelper.NamenNachtragen(TextControl1)

            ' Sidebar auf das neue Dokument umstellen
            ZeigeFormularfelderInSidebar()
        Catch ex As Exception
            MessageBox.Show(
                "Die Datei konnte nicht geöffnet werden:" & vbCrLf & ex.Message,
                "Fehler beim Öffnen",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
            Exit Sub
        End Try

        ' Das frisch geladene Dokument ist der saubere Ausgangsstand. Das Laden
        ' selbst hat Changed ausgelöst, deshalb muss das Zurücksetzen hier
        ' stehen - nach Load und nach NamenNachtragen.
        isDirty = False
        TextControl1.ClearUndo()

        ' Ziel für "Speichern" ohne Rückfrage
        aktuellerPfad = openDlg.FileName
        aktuellerStreamTyp = streamTyp

        ' Die Sicherung bezieht sich ab jetzt auf dieses Dokument
        If autoSpeichern IsNot Nothing Then autoSpeichern.DokumentGewechselt(openDlg.FileName)
        AktualisiereTitelleiste()
    End Sub


    ''' <summary>
    ''' Ermittelt den <see cref="StreamType"/> anhand der Dateiendung.
    ''' </summary>
    Private Function GetStreamTypeFromExtension(dateiName As String) As StreamType
        Select Case System.IO.Path.GetExtension(dateiName).ToLowerInvariant()
            Case ".docx", ".doc"
                Return TXTextControl.StreamType.WordprocessingML
            Case ".rtf"
                Return TXTextControl.StreamType.RichTextFormat
            Case ".html", ".htm"
                Return TXTextControl.StreamType.HTMLFormat
            Case ".txt"
                Return TXTextControl.StreamType.PlainText
            Case ".tx"
                Return TXTextControl.StreamType.InternalUnicodeFormat
            Case Else
                Return TXTextControl.StreamType.WordprocessingML
        End Select
    End Function


    ''' <summary>
    ''' Speichert in die bekannte Datei. Hat das Dokument noch keinen Namen,
    ''' wird nach einem gefragt.
    '''
    ''' Das ist seit der Umstellung kein Nebenschauplatz mehr: Die selbsttätige
    ''' Sicherung fasst die Datei des Anwenders nicht mehr an, nur noch dieser
    ''' Weg schreibt sie.
    ''' </summary>
    Private Sub bt_Save_Click(sender As Object, e As EventArgs) Handles bt_Save.Click

        SpeichereAktuelles()

    End Sub

    ''' <summary>
    ''' Speichert in die bekannte Datei, oder fragt nach einer, falls es noch
    ''' keine gibt.
    ''' </summary>
    ''' <returns>False, wenn abgebrochen wurde oder es misslang.</returns>
    Private Function SpeichereAktuelles() As Boolean

        If String.IsNullOrEmpty(aktuellerPfad) Then Return SpeichernUnter()

        Return Speichere(aktuellerPfad, aktuellerStreamTyp)

    End Function

    ''' <summary>
    ''' Fragt nach Datei und Format und speichert dorthin.
    ''' </summary>
    ''' <returns>False, wenn der Anwender abgebrochen hat oder es misslang.</returns>
    Private Function SpeichernUnter() As Boolean

        Dim saveDlg As New SaveFileDialog()

        saveDlg.Filter = "Word-Dokumente (*.docx)|*.docx|" &
                 "Rich Text Format (*.rtf)|*.rtf|" &
                 "HTML-Dateien (*.html)|*.html|" &
                 "Adobe PDF (*.pdf)|*.pdf|" &
                 "Reiner Text (*.txt)|*.txt"

        If Not String.IsNullOrEmpty(aktuellerPfad) Then
            saveDlg.FileName = System.IO.Path.GetFileName(aktuellerPfad)
        End If

        If saveDlg.ShowDialog() <> DialogResult.OK Then Return False

        Dim streamTyp = StreamType.WordprocessingML
        Select Case saveDlg.FilterIndex
            Case 1 ' .docx
                streamTyp = TXTextControl.StreamType.WordprocessingML
            Case 2 ' .rtf
                streamTyp = TXTextControl.StreamType.RichTextFormat
            Case 3 ' .html
                streamTyp = TXTextControl.StreamType.HTMLFormat
            Case 4 ' .pdf
                streamTyp = TXTextControl.StreamType.AdobePDF
            Case 5 ' .txt
                streamTyp = TXTextControl.StreamType.PlainText
        End Select

        Return Speichere(saveDlg.FileName, streamTyp)

    End Function

    ''' <summary>
    ''' Schreibt das Dokument. Einziger Ort, an dem die Datei des Anwenders
    ''' angefasst wird.
    ''' </summary>
    ''' <returns>False, wenn das Schreiben misslungen ist.</returns>
    Private Function Speichere(pfad As String, streamTyp As StreamType) As Boolean

        Try
            TextControl1.Save(pfad, streamTyp)

        Catch ex As Exception
            MessageBox.Show(
                "Die Datei konnte nicht gespeichert werden:" & vbCrLf & ex.Message,
                "Fehler beim Speichern",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

            Return False
        End Try

        ' PDF, HTML und reiner Text nehmen das Dokument nicht vollständig auf.
        ' Wer dorthin exportiert, hat sein Dokument nicht gesichert - es bleibt
        ' schmutzig, und der bekannte Pfad bleibt der alte.
        If IstVollwertig(streamTyp) Then

            aktuellerPfad = pfad
            aktuellerStreamTyp = streamTyp

            isDirty = False

            If autoSpeichern IsNot Nothing Then autoSpeichern.DokumentGewechselt(pfad)

        End If

        AktualisiereTitelleiste()

        Return True

    End Function

    ''' <summary>
    ''' Formate, die das Dokument vollständig aufnehmen und wieder hergeben.
    ''' </summary>
    Private Shared Function IstVollwertig(streamTyp As StreamType) As Boolean

        Select Case streamTyp
            Case StreamType.WordprocessingML,
                 StreamType.MSWord,
                 StreamType.RichTextFormat,
                 StreamType.InternalUnicodeFormat,
                 StreamType.InternalFormat
                Return True
            Case Else
                Return False
        End Select

    End Function


    ''' <summary>
    ''' Jede Änderung am Dokument macht es schmutzig - ohne Ausnahme.
    '''
    ''' Früher wurde hier ein Hash über TextControl1.Text mit dem Stand beim
    ''' Laden verglichen. Der sieht nur den unformatierten Text: Schriftgröße,
    ''' Fettung, Zellränder ergeben denselben Hash, das Dokument galt trotz
    ''' Änderung als unverändert. Ein Modified-Kennzeichen bietet TX nicht an,
    ''' nur dieses Ereignis - also ist das Ereignis selbst die Wahrheit.
    ''' </summary>
    Private Sub TextControl1_Changed(sender As Object, e As EventArgs) Handles TextControl1.Changed

        isDirty = True

        AktualisiereTitelleiste()
    End Sub

    ''' <summary>
    ''' Bringt die Knöpfe der Titelleiste auf den Stand des Dokumentes.
    ''' </summary>
    Private Sub AktualisiereTitelleiste()

        ' Seit die selbsttätige Sicherung die Datei des Anwenders nicht mehr
        ' mitschreibt, muss sichtbar sein, worauf "Speichern" zielt und ob es
        ' etwas zu speichern gibt. Der Vergleich davor spart das Neuzeichnen
        ' der Titelzeile bei jedem Tastendruck.
        Dim titel = If(String.IsNullOrEmpty(aktuellerPfad),
                       "Unbenanntes Dokument",
                       System.IO.Path.GetFileName(aktuellerPfad)) &
                    If(isDirty, " *", "")

        If Me.Text <> titel Then Me.Text = titel

        bt_SaveTitleBar.Enabled = isDirty
        bt_UndoTitleBar.Enabled = TextControl1.CanUndo
        bt_RedoTitleBar.Enabled = TextControl1.CanRedo
    End Sub

    Private Sub FuegeBausteinEin(platzhalterName As String, folder As String)

        Dim pfad As String = System.IO.Path.Combine(
            Application.StartupPath, "..", "..", "app_data", folder, platzhalterName)

        If Not System.IO.File.Exists(pfad) Then Exit Sub

        ' Einfuegen aller Textbausteine aus einem Ordner
        ' Bausteinhelper.AlleBausteineEinfuegen(TextControl1, "C:\Quality Bytes\Projekte\Entsorgungsbetriebe_Luebeck\Projekte\Demoprojekt\app_data\Hinweis")

        ' Einzelnen Textbaustein einfügen
        Bausteinhelper.BausteineEinfuegen(TextControl1, {pfad})
    End Sub




#Region "Sidebar - Formularfelder"

    ''' <summary>
    ''' Hängt das UserControl in die Sidebar.
    ''' ContentLayout muss auf Custom stehen, sonst ignoriert die Sidebar den
    ''' zugewiesenen Content. Dock = Fill, sonst bleibt das Control auf seiner
    ''' Entwurfsgröße stehen und die Sidebar ist darunter leer.
    ''' </summary>
    Private Sub CreateSideBar()

        ucFormularfelder = New UcFormularfelder() With {.Dock = DockStyle.Fill}

        Sidebar2.Text = "Formularfelder"
        Sidebar2.ShowTitle = True
        Sidebar2.ContentLayout = TXTextControl.Windows.Forms.Sidebar.SidebarContentLayout.Custom
        Sidebar2.Content = ucFormularfelder

        ZeigeFormularfelderInSidebar()
    End Sub

    ''' <summary>
    ''' Liest den Ist-Zustand aus dem Dokument in die Sidebar.
    ''' Nach jedem Laden aufrufen, sonst zeigt die Sidebar die Felder des
    ''' vorherigen Dokumentes.
    ''' </summary>
    Private Sub ZeigeFormularfelderInSidebar()

        If ucFormularfelder Is Nothing Then Exit Sub

        ucFormularfelder.Daten = FormularHelper.Checkboxzustaende(TextControl1)
    End Sub

    ''' <summary>
    ''' Ein Häkchen in der Sidebar wurde umgeschaltet - sofort ins Dokument
    ''' schreiben, damit Sidebar und Dokument nicht auseinanderlaufen.
    ''' </summary>
    Private Sub ucFormularfelder_FeldGeaendert(sender As Object, e As FeldGeaendertEventArgs) Handles ucFormularfelder.FeldGeaendert

        If FormularHelper.SetzeCheckbox(TextControl1, e.Feldname, e.Aktiviert) Then Exit Sub

        MessageBox.Show(
            "Das Feld '" & e.Feldname & "' ist im Dokument nicht mehr vorhanden.",
            "Formularfeld nicht gefunden",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning)

        ' Sidebar wieder auf den Stand des Dokumentes bringen
        ZeigeFormularfelderInSidebar()
    End Sub

    ''' <summary>
    ''' Der Anwender hat in der Sidebar "Aus Dokument neu einlesen" gedrückt.
    ''' Nötig, wenn die Häkchen direkt im Dokument gesetzt wurden.
    ''' </summary>
    Private Sub ucFormularfelder_AktualisierungAngefordert(sender As Object, e As EventArgs) Handles ucFormularfelder.AktualisierungAngefordert
        ZeigeFormularfelderInSidebar()
    End Sub


    ''' <summary>
    ''' Holt die Sidebar zurück - angezeigt und angedockt.
    '''
    ''' Gebraucht wird das für den Fall, dass die Sidebar im angedockten
    ''' Zustand über ihr Kreuz geschlossen wird. Dann steht IsShown auf False,
    ''' es existiert kein freigestelltes Fenster, und in der Oberfläche gibt
    ''' es keinen Weg zurück.
    ''' </summary>
    Private Sub ZeigeSidebar()

        Sidebar2.IsShown = True
        Sidebar2.IsPinned = True
    End Sub

    Private Sub bt_Sidebar_Click(sender As Object, e As EventArgs)
        ZeigeSidebar()
    End Sub

    ''' <summary>
    ''' Beim Abdocken hängt TX das UserControl in ein eigenes Fenster um.
    ''' Einen Event dafür bietet die Sidebar nicht - der Wechsel des
    ''' Elternfensters ist der einzige Weg, davon zu erfahren.
    ''' </summary>
    Private Sub ucFormularfelder_ParentChanged(sender As Object, e As EventArgs) Handles ucFormularfelder.ParentChanged

        Dim parent = ucFormularfelder.ParentForm

        If parent Is Nothing OrElse parent Is Me Then Exit Sub
        If sidebarFenster.Contains(parent) Then Exit Sub

        sidebarFenster.Add(parent)
        AddHandler parent.FormClosed, AddressOf SidebarFenster_FormClosed
    End Sub

    ''' <summary>
    ''' Das freigestellte Sidebar-Fenster wurde geschlossen. Statt die Sidebar
    ''' verschwinden zu lassen, wandert sie zurück ins Hauptfenster.
    '''
    ''' Bewusst FormClosed und nicht FormClosing: Ein per e.Cancel
    ''' abgebrochenes FormClosing greift auch beim Beenden der Anwendung und
    ''' verhindert dann, dass sich das Programm schließen lässt.
    ''' </summary>
    Private Sub SidebarFenster_FormClosed(sender As Object, e As FormClosedEventArgs)

        If isFinished Then Exit Sub

        ZeigeSidebar()
    End Sub

    ''' <summary>
    ''' Beim Beenden muss nachgefragt werden, und zwar erst seit der
    ''' Umstellung zwingend: Früher hat die selbsttätige Sicherung die Datei
    ''' des Anwenders mitgeschrieben, heute nicht mehr. Ohne diese Abfrage wäre
    ''' das Schliessen ein stiller Totalverlust.
    '''
    ''' Ein letztes Sichern gibt es nicht - die Wiederherstellungsdatei wird
    ''' beim sauberen Beenden ja gerade weggeräumt. Ihr Vorhandensein ist das
    ''' Kennzeichen dafür, dass eine Sitzung nicht zu Ende ging.
    ''' </summary>
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        If isDirty Then

            Dim antwort = MessageBox.Show(
                "Das Dokument enthält ungespeicherte Änderungen." & vbCrLf & vbCrLf &
                "Möchten Sie sie speichern?",
                "Ungespeicherte Änderungen",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning)

            ' Abbrechen lässt alles stehen, auch die Wiederherstellungsdatei.
            If antwort = DialogResult.Cancel Then
                e.Cancel = True
                Exit Sub
            End If

            If antwort = DialogResult.Yes AndAlso Not SpeichereAktuelles() Then
                e.Cancel = True
                Exit Sub
            End If

        End If

        isFinished = True

        ' Auch ein übernommener, aber noch nie gesicherter Stand wird jetzt
        ' weggeräumt. Der Anwender hat eben entschieden, was damit geschieht -
        ' beim nächsten Start noch einmal danach zu fragen wäre zudringlich.
        UebernahmeAbschliessen()

        If autoSpeichern IsNot Nothing Then autoSpeichern.SauberBeenden()

    End Sub
#End Region

End Class
