<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits TXTextControl.Windows.Forms.Ribbon.RibbonForm

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.Ribbon1 = New TXTextControl.Windows.Forms.Ribbon.Ribbon()
        Me.bt_Open = New TXTextControl.Windows.Forms.Ribbon.RibbonButton()
        Me.bt_Save = New TXTextControl.Windows.Forms.Ribbon.RibbonButton()
        Me.KontextTabWerkz = New TXTextControl.Windows.Forms.Ribbon.ContextualTabGroup()
        Me.RibbonTableLayoutTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonTableLayoutTab()
        Me.KontextRahmenWerkz = New TXTextControl.Windows.Forms.Ribbon.ContextualTabGroup()
        Me.RibbonFrameLayoutTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonFrameLayoutTab()
        Me.RibbonFormattingTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonFormattingTab()
        Me.RibbonInsertTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonInsertTab()
        Me.RibbonPageLayoutTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonPageLayoutTab()
        Me.RibbonReferencesTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonReferencesTab()
        Me.RibbonFormFieldsTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonFormFieldsTab()
        Me.RibbonViewTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonViewTab()
        Me.RibbonProofingTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonProofingTab()
        Me.RibbonPermissionsTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonPermissionsTab()
        Me.RibbonReportingTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonReportingTab()
        Me.RibbonTab1 = New TXTextControl.Windows.Forms.Ribbon.RibbonTab()
        Me.StatusBar1 = New TXTextControl.StatusBar()
        Me.RulerBar1 = New TXTextControl.RulerBar()
        Me.RulerBar2 = New TXTextControl.RulerBar()
        Me.Sidebar1 = New TXTextControl.Windows.Forms.Sidebar()
        Me.TextControl1 = New TXTextControl.TextControl()
        Me.ServerTextControl1 = New TXTextControl.ServerTextControl()
        Me.Sidebar2 = New TXTextControl.Windows.Forms.Sidebar()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.lbl_Sicherung = New System.Windows.Forms.ToolStripStatusLabel()
        Me.TxSpellChecker1 = New TXTextControl.Proofing.TXSpellChecker()
        Me.RibbonProofingTab2 = New TXTextControl.Windows.Forms.Ribbon.RibbonProofingTab()
        Me.Ribbon1.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Ribbon1
        '
        Me.Ribbon1.ApplicationMenuItems.AddRange(New System.Windows.Forms.Control() {Me.bt_Open, Me.bt_Save})
        Me.Ribbon1.ContextualTabGroups.Add(Me.KontextTabWerkz)
        Me.Ribbon1.ContextualTabGroups.Add(Me.KontextRahmenWerkz)
        Me.Ribbon1.Controls.Add(Me.RibbonFormattingTab1)
        Me.Ribbon1.Controls.Add(Me.RibbonInsertTab1)
        Me.Ribbon1.Controls.Add(Me.RibbonPageLayoutTab1)
        Me.Ribbon1.Controls.Add(Me.RibbonReferencesTab1)
        Me.Ribbon1.Controls.Add(Me.RibbonFormFieldsTab1)
        Me.Ribbon1.Controls.Add(Me.RibbonViewTab1)
        Me.Ribbon1.Controls.Add(Me.RibbonProofingTab1)
        Me.Ribbon1.Controls.Add(Me.RibbonPermissionsTab1)
        Me.Ribbon1.Controls.Add(Me.RibbonReportingTab1)
        Me.Ribbon1.Controls.Add(Me.RibbonTab1)
        Me.Ribbon1.Controls.Add(Me.RibbonProofingTab2)
        Me.Ribbon1.DisplayColors.ApplicationMenuColor = System.Drawing.Color.WhiteSmoke
        Me.Ribbon1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Ribbon1.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Ribbon1.HotTrack = True
        Me.Ribbon1.Location = New System.Drawing.Point(0, 31)
        Me.Ribbon1.Name = "Ribbon1"
        Me.Ribbon1.SelectedIndex = 11
        Me.Ribbon1.Size = New System.Drawing.Size(1144, 118)
        Me.Ribbon1.TabIndex = 1
        Me.Ribbon1.Text = "Ribbon1"
        '
        'bt_Open
        '
        Me.bt_Open.BackColor = System.Drawing.Color.Transparent
        Me.bt_Open.Dock = System.Windows.Forms.DockStyle.Top
        Me.bt_Open.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.bt_Open.KeyTip = ""
        Me.bt_Open.Location = New System.Drawing.Point(0, 0)
        Me.bt_Open.Margin = New System.Windows.Forms.Padding(0, 0, 1, 0)
        Me.bt_Open.Name = "bt_Open"
        Me.bt_Open.Size = New System.Drawing.Size(191, 38)
        Me.bt_Open.TabIndex = 0
        Me.bt_Open.Text = "Öffnen ..."
        '
        'bt_Save
        '
        Me.bt_Save.BackColor = System.Drawing.Color.Transparent
        Me.bt_Save.Dock = System.Windows.Forms.DockStyle.Top
        Me.bt_Save.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.bt_Save.KeyTip = ""
        Me.bt_Save.Location = New System.Drawing.Point(0, 38)
        Me.bt_Save.Margin = New System.Windows.Forms.Padding(0, 0, 1, 0)
        Me.bt_Save.Name = "bt_Save"
        Me.bt_Save.Size = New System.Drawing.Size(191, 38)
        Me.bt_Save.TabIndex = 0
        Me.bt_Save.Text = "Speichern"
        '
        'KontextTabWerkz
        '
        Me.KontextTabWerkz.BackColor = System.Drawing.Color.LightGray
        Me.KontextTabWerkz.ContextualTabs.Add(Me.RibbonTableLayoutTab1)
        Me.KontextTabWerkz.Name = "KontextTabWerkz"
        '
        'RibbonTableLayoutTab1
        '
        Me.RibbonTableLayoutTab1.Location = New System.Drawing.Point(0, 0)
        Me.RibbonTableLayoutTab1.Name = "RibbonTableLayoutTab1"
        Me.RibbonTableLayoutTab1.Size = New System.Drawing.Size(200, 40)
        Me.RibbonTableLayoutTab1.TabIndex = 0
        '
        'KontextRahmenWerkz
        '
        Me.KontextRahmenWerkz.BackColor = System.Drawing.Color.LightGray
        Me.KontextRahmenWerkz.ContextualTabs.Add(Me.RibbonFrameLayoutTab1)
        Me.KontextRahmenWerkz.Name = "KontextRahmenWerkz"
        '
        'RibbonFrameLayoutTab1
        '
        Me.RibbonFrameLayoutTab1.Location = New System.Drawing.Point(0, 0)
        Me.RibbonFrameLayoutTab1.Name = "RibbonFrameLayoutTab1"
        Me.RibbonFrameLayoutTab1.Size = New System.Drawing.Size(200, 40)
        Me.RibbonFrameLayoutTab1.TabIndex = 0
        '
        'RibbonFormattingTab1
        '
        Me.RibbonFormattingTab1.Location = New System.Drawing.Point(4, 24)
        Me.RibbonFormattingTab1.Name = "RibbonFormattingTab1"
        Me.RibbonFormattingTab1.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonFormattingTab1.TabIndex = 1
        '
        'RibbonInsertTab1
        '
        Me.RibbonInsertTab1.Location = New System.Drawing.Point(4, 24)
        Me.RibbonInsertTab1.Name = "RibbonInsertTab1"
        Me.RibbonInsertTab1.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonInsertTab1.TabIndex = 2
        '
        'RibbonPageLayoutTab1
        '
        Me.RibbonPageLayoutTab1.Location = New System.Drawing.Point(4, 24)
        Me.RibbonPageLayoutTab1.Name = "RibbonPageLayoutTab1"
        Me.RibbonPageLayoutTab1.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonPageLayoutTab1.TabIndex = 3
        '
        'RibbonReferencesTab1
        '
        Me.RibbonReferencesTab1.Location = New System.Drawing.Point(4, 24)
        Me.RibbonReferencesTab1.Name = "RibbonReferencesTab1"
        Me.RibbonReferencesTab1.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonReferencesTab1.TabIndex = 9
        '
        'RibbonFormFieldsTab1
        '
        Me.RibbonFormFieldsTab1.Location = New System.Drawing.Point(4, 24)
        Me.RibbonFormFieldsTab1.Name = "RibbonFormFieldsTab1"
        Me.RibbonFormFieldsTab1.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonFormFieldsTab1.TabIndex = 8
        '
        'RibbonViewTab1
        '
        Me.RibbonViewTab1.Location = New System.Drawing.Point(4, 24)
        Me.RibbonViewTab1.Name = "RibbonViewTab1"
        Me.RibbonViewTab1.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonViewTab1.TabIndex = 4
        '
        'RibbonProofingTab1
        '
        Me.RibbonProofingTab1.Location = New System.Drawing.Point(4, 24)
        Me.RibbonProofingTab1.Name = "RibbonProofingTab1"
        Me.RibbonProofingTab1.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonProofingTab1.TabIndex = 5
        '
        'RibbonPermissionsTab1
        '
        Me.RibbonPermissionsTab1.AllowAddingUserNames = True
        Me.RibbonPermissionsTab1.Location = New System.Drawing.Point(4, 24)
        Me.RibbonPermissionsTab1.Name = "RibbonPermissionsTab1"
        Me.RibbonPermissionsTab1.RegisteredUserNames = New String(-1) {}
        Me.RibbonPermissionsTab1.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonPermissionsTab1.TabIndex = 6
        '
        'RibbonReportingTab1
        '
        Me.RibbonReportingTab1.Location = New System.Drawing.Point(4, 24)
        Me.RibbonReportingTab1.Name = "RibbonReportingTab1"
        Me.RibbonReportingTab1.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonReportingTab1.TabIndex = 7
        '
        'RibbonTab1
        '
        Me.RibbonTab1.KeyTip = ""
        Me.RibbonTab1.Location = New System.Drawing.Point(4, 24)
        Me.RibbonTab1.Name = "RibbonTab1"
        Me.RibbonTab1.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonTab1.TabIndex = 10
        Me.RibbonTab1.Text = "Mein Ribbon"
        '
        'StatusBar1
        '
        Me.StatusBar1.BackColor = System.Drawing.SystemColors.Control
        Me.StatusBar1.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.StatusBar1.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.StatusBar1.Location = New System.Drawing.Point(0, 1168)
        Me.StatusBar1.Margin = New System.Windows.Forms.Padding(2)
        Me.StatusBar1.Name = "StatusBar1"
        Me.StatusBar1.Size = New System.Drawing.Size(1144, 22)
        Me.StatusBar1.TabIndex = 2
        '
        'RulerBar1
        '
        Me.RulerBar1.Alignment = TXTextControl.RulerBarAlignment.Left
        Me.RulerBar1.Dock = System.Windows.Forms.DockStyle.Left
        Me.RulerBar1.Location = New System.Drawing.Point(130, 174)
        Me.RulerBar1.Margin = New System.Windows.Forms.Padding(2)
        Me.RulerBar1.Name = "RulerBar1"
        Me.RulerBar1.Size = New System.Drawing.Size(25, 994)
        Me.RulerBar1.TabIndex = 3
        Me.RulerBar1.Text = "RulerBar1"
        '
        'RulerBar2
        '
        Me.RulerBar2.Dock = System.Windows.Forms.DockStyle.Top
        Me.RulerBar2.Location = New System.Drawing.Point(130, 149)
        Me.RulerBar2.Margin = New System.Windows.Forms.Padding(2)
        Me.RulerBar2.Name = "RulerBar2"
        Me.RulerBar2.Size = New System.Drawing.Size(783, 25)
        Me.RulerBar2.TabIndex = 4
        Me.RulerBar2.Text = "RulerBar2"
        '
        'Sidebar1
        '
        Me.Sidebar1.ContentLayout = TXTextControl.Windows.Forms.Sidebar.SidebarContentLayout.Styles
        Me.Sidebar1.Dock = System.Windows.Forms.DockStyle.Right
        Me.Sidebar1.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Sidebar1.Location = New System.Drawing.Point(913, 149)
        Me.Sidebar1.Margin = New System.Windows.Forms.Padding(2)
        Me.Sidebar1.Name = "Sidebar1"
        Me.Sidebar1.Padding = New System.Windows.Forms.Padding(3)
        Me.Sidebar1.Size = New System.Drawing.Size(231, 1019)
        Me.Sidebar1.TabIndex = 5
        Me.Sidebar1.TextControl = Me.TextControl1
        '
        'TextControl1
        '
        Me.TextControl1.DisplayColors.FormFieldColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.TextControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TextControl1.Font = New System.Drawing.Font("Arial", 10.0!)
        Me.TextControl1.FormattingPrinter = "Standard"
        Me.TextControl1.IsHyphenationEnabled = True
        Me.TextControl1.IsLanguageDetectionEnabled = True
        Me.TextControl1.IsSpellCheckingEnabled = True
        Me.TextControl1.Location = New System.Drawing.Point(155, 174)
        Me.TextControl1.Margin = New System.Windows.Forms.Padding(2)
        Me.TextControl1.Name = "TextControl1"
        Me.TextControl1.PageMargins.Bottom = 78.75R
        Me.TextControl1.PageMargins.Left = 78.75R
        Me.TextControl1.PageMargins.Right = 78.75R
        Me.TextControl1.PageMargins.Top = 78.75R
        Me.TextControl1.PageSize.Height = 1169.31R
        Me.TextControl1.PageSize.Width = 826.81R
        Me.TextControl1.Ribbon = Me.Ribbon1
        Me.TextControl1.RulerBar = Me.RulerBar2
        Me.TextControl1.Size = New System.Drawing.Size(758, 994)
        Me.TextControl1.SpellChecker = Me.TxSpellChecker1
        Me.TextControl1.StatusBar = Me.StatusBar1
        Me.TextControl1.TabIndex = 0
        Me.TextControl1.UserNames = Nothing
        Me.TextControl1.VerticalRulerBar = Me.RulerBar1
        '
        'ServerTextControl1
        '
        Me.ServerTextControl1.FormattingPrinter = "Standard"
        Me.ServerTextControl1.SpellChecker = Nothing
        '
        'Sidebar2
        '
        Me.Sidebar2.DialogStyle = TXTextControl.Windows.Forms.Sidebar.SidebarDialogStyle.StandardSizable
        Me.Sidebar2.Dock = System.Windows.Forms.DockStyle.Left
        Me.Sidebar2.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Sidebar2.Location = New System.Drawing.Point(0, 149)
        Me.Sidebar2.Name = "Sidebar2"
        Me.Sidebar2.Size = New System.Drawing.Size(130, 1019)
        Me.Sidebar2.TabIndex = 6
        Me.Sidebar2.Text = "Sidebar2"
        Me.Sidebar2.TextControl = Me.TextControl1
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lbl_Sicherung})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 1190)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(1144, 22)
        Me.StatusStrip1.SizingGrip = False
        Me.StatusStrip1.TabIndex = 7
        '
        'lbl_Sicherung
        '
        Me.lbl_Sicherung.Name = "lbl_Sicherung"
        Me.lbl_Sicherung.Size = New System.Drawing.Size(0, 17)
        '
        'TxSpellChecker1
        '
        Me.TxSpellChecker1.DetectableLanguageScopes = New System.Globalization.CultureInfo() {New System.Globalization.CultureInfo("ar"), New System.Globalization.CultureInfo("de"), New System.Globalization.CultureInfo("en"), New System.Globalization.CultureInfo("es"), New System.Globalization.CultureInfo("fr"), New System.Globalization.CultureInfo("pt"), New System.Globalization.CultureInfo("ru")}
        '
        'RibbonProofingTab2
        '
        Me.RibbonProofingTab2.Location = New System.Drawing.Point(4, 24)
        Me.RibbonProofingTab2.Name = "RibbonProofingTab2"
        Me.RibbonProofingTab2.Size = New System.Drawing.Size(1136, 90)
        Me.RibbonProofingTab2.TabIndex = 11
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1144, 1212)
        Me.Controls.Add(Me.TextControl1)
        Me.Controls.Add(Me.RulerBar1)
        Me.Controls.Add(Me.RulerBar2)
        Me.Controls.Add(Me.Sidebar2)
        Me.Controls.Add(Me.Sidebar1)
        Me.Controls.Add(Me.StatusBar1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.Ribbon1)
        Me.Margin = New System.Windows.Forms.Padding(2)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.Ribbon1.ResumeLayout(False)
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Ribbon1 As TXTextControl.Windows.Forms.Ribbon.Ribbon
    Friend WithEvents RulerBar2 As TXTextControl.RulerBar
    Friend WithEvents StatusBar1 As TXTextControl.StatusBar
    Friend WithEvents RulerBar1 As TXTextControl.RulerBar
    Friend WithEvents Sidebar1 As TXTextControl.Windows.Forms.Sidebar
    Friend WithEvents TextControl1 As TXTextControl.TextControl
    Friend WithEvents RibbonFormattingTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonFormattingTab
    Friend WithEvents RibbonInsertTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonInsertTab
    Friend WithEvents RibbonPageLayoutTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonPageLayoutTab
    Friend WithEvents RibbonViewTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonViewTab
    Friend WithEvents RibbonProofingTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonProofingTab
    Friend WithEvents RibbonPermissionsTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonPermissionsTab
    Friend WithEvents RibbonReportingTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonReportingTab
    Friend WithEvents RibbonFormFieldsTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonFormFieldsTab
    Friend WithEvents RibbonReferencesTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonReferencesTab
    Friend WithEvents RibbonTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonTab
    Friend WithEvents KontextTabWerkz As TXTextControl.Windows.Forms.Ribbon.ContextualTabGroup
    Friend WithEvents RibbonTableLayoutTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonTableLayoutTab
    Friend WithEvents KontextRahmenWerkz As TXTextControl.Windows.Forms.Ribbon.ContextualTabGroup
    Friend WithEvents RibbonFrameLayoutTab1 As TXTextControl.Windows.Forms.Ribbon.RibbonFrameLayoutTab
    Friend WithEvents bt_Open As TXTextControl.Windows.Forms.Ribbon.RibbonButton
    Friend WithEvents bt_Save As TXTextControl.Windows.Forms.Ribbon.RibbonButton
    Friend WithEvents ServerTextControl1 As TXTextControl.ServerTextControl
    Friend WithEvents Sidebar2 As TXTextControl.Windows.Forms.Sidebar
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents lbl_Sicherung As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents TxSpellChecker1 As TXTextControl.Proofing.TXSpellChecker
    Friend WithEvents RibbonProofingTab2 As TXTextControl.Windows.Forms.Ribbon.RibbonProofingTab
End Class
