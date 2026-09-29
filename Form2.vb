Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class Form2
    Public Property RueckgabeText As String = ""

    Private Sub btnUebernehmen_Click(sender As Object, e As EventArgs) Handles btnUebernehmen.Click
        ' Wert aus z.B. einer TextBox in der Eigenschaft speichern
        Me.RueckgabeText = TextBox1.Text

        ' DialogResult auf OK setzen – das schließt das Fenster automatisch
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub btnAbbrechen_Click(sender As Object, e As EventArgs) Handles btnAbbrechen.Click
        ' Bei Abbrechen einfach mit Cancel schließen
        Me.DialogResult = DialogResult.Cancel
    End Sub
End Class