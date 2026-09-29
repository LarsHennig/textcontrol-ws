<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UcFormularfelder
    Inherits System.Windows.Forms.UserControl

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
        Me.lblKopf = New System.Windows.Forms.Label()
        Me.gitter = New System.Windows.Forms.DataGridView()
        Me.spalteFeld = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.spalteGesetzt = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.btAktualisieren = New System.Windows.Forms.Button()
        CType(Me.gitter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblKopf
        '
        Me.lblKopf.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblKopf.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.lblKopf.Location = New System.Drawing.Point(0, 0)
        Me.lblKopf.Name = "lblKopf"
        Me.lblKopf.Padding = New System.Windows.Forms.Padding(2, 4, 2, 4)
        Me.lblKopf.Size = New System.Drawing.Size(250, 25)
        Me.lblKopf.TabIndex = 0
        Me.lblKopf.Text = "Formularfelder"
        '
        'gitter
        '
        Me.gitter.AllowUserToAddRows = False
        Me.gitter.AllowUserToDeleteRows = False
        Me.gitter.AllowUserToResizeRows = False
        Me.gitter.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gitter.BackgroundColor = System.Drawing.SystemColors.Window
        Me.gitter.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.gitter.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gitter.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.spalteFeld, Me.spalteGesetzt})
        Me.gitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gitter.Location = New System.Drawing.Point(0, 25)
        Me.gitter.MultiSelect = False
        Me.gitter.Name = "gitter"
        Me.gitter.RowHeadersVisible = False
        Me.gitter.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.gitter.Size = New System.Drawing.Size(250, 325)
        Me.gitter.TabIndex = 1
        '
        'spalteFeld
        '
        Me.spalteFeld.FillWeight = 70.0!
        Me.spalteFeld.HeaderText = "Feld"
        Me.spalteFeld.Name = "spalteFeld"
        Me.spalteFeld.ReadOnly = True
        '
        'spalteGesetzt
        '
        Me.spalteGesetzt.FillWeight = 30.0!
        Me.spalteGesetzt.HeaderText = "Gesetzt"
        Me.spalteGesetzt.Name = "spalteGesetzt"
        '
        'btAktualisieren
        '
        Me.btAktualisieren.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.btAktualisieren.Location = New System.Drawing.Point(0, 350)
        Me.btAktualisieren.Name = "btAktualisieren"
        Me.btAktualisieren.Size = New System.Drawing.Size(250, 30)
        Me.btAktualisieren.TabIndex = 2
        Me.btAktualisieren.Text = "Aus Dokument neu einlesen"
        Me.btAktualisieren.UseVisualStyleBackColor = True
        '
        'UcFormularfelder
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.gitter)
        Me.Controls.Add(Me.lblKopf)
        Me.Controls.Add(Me.btAktualisieren)
        Me.Name = "UcFormularfelder"
        Me.Size = New System.Drawing.Size(250, 380)
        CType(Me.gitter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblKopf As Label
    Friend WithEvents gitter As DataGridView
    Friend WithEvents spalteFeld As DataGridViewTextBoxColumn
    Friend WithEvents spalteGesetzt As DataGridViewCheckBoxColumn
    Friend WithEvents btAktualisieren As Button
End Class
