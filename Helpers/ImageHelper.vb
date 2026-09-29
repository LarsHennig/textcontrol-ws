Imports TXTextControl
Imports TXTextControl.Windows.Forms
Imports TXTextControl.Windows.Forms.ResourceProvider

' https://www.textcontrol.com/blog/2024/05/03/various-ways-of-inserting-images-into-tx-text-control/

Public Module ImageHelper


    ''' <summary>
    ''' Lädt ein Icon (Large) aus den Icon Resourcen von TextControl
    ''' </summary>
    ''' <param name="identifier"></param>
    ''' <param name="dpi"></param>
    ''' <returns>Geladene Bitmap</returns>
    Public Function GetTextControlLargeIcon(identifier As String, dpi As Integer) As Bitmap

        Dim settings As New ImageSourceSettings()
        settings.Culture = System.Globalization.CultureInfo.CurrentUICulture

        Return ResourceProvider.GetLargeIcon(identifier, dpi, settings)
    End Function


    ''' <summary>
    ''' Lädt ein Icon (Small) aus den Icon Resourcen von TextControl
    ''' </summary>
    ''' <param name="identifier"></param>
    ''' <param name="dpi"></param>
    ''' <returns>Geladene Bitmap</returns>
    Public Function GetTextControlSmallIcon(identifier As String, dpi As Integer) As Bitmap

        Dim settings As New ImageSourceSettings()
        settings.Culture = System.Globalization.CultureInfo.CurrentUICulture

        Return ResourceProvider.GetSmallIcon(identifier, dpi, settings)
    End Function


    Public Sub LoadImageFromFile(filename As String, textControl As TXTextControl.TextControl, Optional pos As Point = Nothing)

        Dim image As New TXTextControl.Image()
        image.FileName = filename

        If filename Is Nothing Then
            textControl.Images.Add(image, -1)
            Return
        End If

        textControl.Images.Add(image, pos, -1, TXTextControl.ImageInsertionMode.MoveWithText Or TXTextControl.ImageInsertionMode.DisplaceText)

    End Sub
End Module
