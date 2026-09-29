''' <summary>
''' Sidebar-Inhalt: zeigt die Formularfelder des geladenen Dokumentes und
''' lässt sie anhaken.
'''
''' Das Control kennt weder das TextControl noch ein bestimmtes Vertragsmuster.
''' Es bekommt die Felder über die Eigenschaft Daten und meldet Änderungen per
''' Event nach außen - die Hauptanwendung entscheidet, was damit geschieht.
''' </summary>
Public Class UcFormularfelder

    ''' <summary>Ein Feld im Gitter wurde vom Anwender umgeschaltet.</summary>
    Public Event FeldGeaendert As EventHandler(Of FeldGeaendertEventArgs)

    ''' <summary>Der Anwender hat "Aktualisieren" gedrückt.</summary>
    Public Event AktualisierungAngefordert As EventHandler

    ''' <summary>
    ''' Während das Gitter aus Daten befüllt wird, dürfen die Zellen keine
    ''' Änderungsmeldung auslösen - sonst meldet schon das Anzeigen eine
    ''' Änderung, die der Anwender nie gemacht hat.
    ''' </summary>
    Private befuellen As Boolean = False

    ''' <summary>
    ''' Feldname und Zustand, in Dokumentreihenfolge.
    ''' Setzen füllt das Gitter, Lesen gibt den aktuellen Stand zurück.
    ''' </summary>
    Public Property Daten As List(Of KeyValuePair(Of String, Boolean))
        Get
            Dim ergebnis As New List(Of KeyValuePair(Of String, Boolean))

            For Each zeile As DataGridViewRow In gitter.Rows
                ergebnis.Add(New KeyValuePair(Of String, Boolean)(
                    Convert.ToString(zeile.Cells(spalteFeld.Index).Value),
                    Convert.ToBoolean(zeile.Cells(spalteGesetzt.Index).Value)))
            Next

            Return ergebnis
        End Get
        Set(wert As List(Of KeyValuePair(Of String, Boolean)))

            befuellen = True

            Try
                gitter.Rows.Clear()

                If wert IsNot Nothing Then
                    For Each eintrag In wert
                        gitter.Rows.Add(eintrag.Key, eintrag.Value)
                    Next
                End If

                ZeigeAnzahl()

            Finally
                befuellen = False
            End Try

        End Set
    End Property

    ''' <summary>
    ''' Setzt den Zustand eines einzelnen Feldes, ohne ein Event auszulösen.
    ''' Für den Fall, dass sich der Wert im Dokument geändert hat.
    ''' </summary>
    Public Sub AktualisiereFeld(feldname As String, aktiviert As Boolean)

        befuellen = True

        Try
            For Each zeile As DataGridViewRow In gitter.Rows
                If String.Equals(Convert.ToString(zeile.Cells(spalteFeld.Index).Value),
                                 feldname, StringComparison.OrdinalIgnoreCase) Then
                    zeile.Cells(spalteGesetzt.Index).Value = aktiviert
                    Exit For
                End If
            Next
        Finally
            befuellen = False
        End Try

    End Sub

    ''' <summary>
    ''' Ein Häkchen in einer DataGridViewCheckBoxCell wird erst übernommen,
    ''' wenn die Zelle verlassen wird. Damit das Event sofort beim Klick
    ''' kommt, wird die Bearbeitung hier direkt festgeschrieben.
    ''' </summary>
    Private Sub gitter_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs) Handles gitter.CurrentCellDirtyStateChanged

        If gitter.IsCurrentCellDirty Then
            gitter.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub

    Private Sub gitter_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles gitter.CellValueChanged

        If befuellen Then Exit Sub
        If e.RowIndex < 0 OrElse e.ColumnIndex <> spalteGesetzt.Index Then Exit Sub

        Dim zeile = gitter.Rows(e.RowIndex)

        RaiseEvent FeldGeaendert(Me, New FeldGeaendertEventArgs(
            Convert.ToString(zeile.Cells(spalteFeld.Index).Value),
            Convert.ToBoolean(zeile.Cells(spalteGesetzt.Index).Value)))
    End Sub

    Private Sub btAktualisieren_Click(sender As Object, e As EventArgs) Handles btAktualisieren.Click
        RaiseEvent AktualisierungAngefordert(Me, EventArgs.Empty)
    End Sub

    Private Sub ZeigeAnzahl()

        If gitter.Rows.Count = 0 Then
            lblKopf.Text = "Keine Formularfelder im Dokument"
        Else
            lblKopf.Text = "Formularfelder (" & gitter.Rows.Count & ")"
        End If
    End Sub

End Class

''' <summary>
''' Meldet, welches Feld auf welchen Wert umgeschaltet wurde.
''' </summary>
Public Class FeldGeaendertEventArgs
    Inherits EventArgs

    Public ReadOnly Property Feldname As String
    Public ReadOnly Property Aktiviert As Boolean

    Public Sub New(feldname As String, aktiviert As Boolean)
        Me.Feldname = feldname
        Me.Aktiviert = aktiviert
    End Sub

End Class
