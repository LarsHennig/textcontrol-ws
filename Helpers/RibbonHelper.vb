Imports TXTextControl.Windows.Forms.Ribbon

Public Module RibbonHelper

#Region "Öffentliche Funktionen"

    ''' <summary>
    ''' Erstellt einen RibbonButton (Large) <br/>
    ''' Lädt nur TextControl Icons 
    ''' </summary>
    ''' <param name="buttonText">Anzeigetext</param>
    ''' <param name="identifier">Bezeichner für die Icon-Resource von Textcontrol</param>
    ''' <param name="dpi">Dots per Inch (Punktdichte)</param>
    ''' <param name="clickHandler">Handler für das Klick-Event</param>
    ''' <returns></returns>
    Public Function CreateRibbonButtonSmall(buttonText As String, identifier As String, dpi As Integer, clickHandler As EventHandler) As RibbonButton

        Dim bt_control = New RibbonButton() With
        {
            .Text = buttonText,
            .DisplayMode = IconTextRelation.SmallIconLabeled,
            .SmallIcon = GetTextControlSmallIcon(identifier, dpi)
        }

        AddHandler bt_control.Click, clickHandler

        Return bt_control
    End Function


    ''' <summary>
    ''' Erstellt einen RibbonButton (Small) ohne Text
    ''' </summary>
    ''' <param name="identifier">Bezeichner für die Icon-Resource von Textcontrol</param>
    ''' <param name="dpi">Dots per Inch (Punktdichte)</param>
    ''' <param name="clickHandler">Handler für das Klick-Event</param>
    Public Function CreateRibbonButtonSmallNonLabeld(identifier As String, dpi As Integer, clickHandler As EventHandler) As RibbonButton
        Dim bt_control = New RibbonButton() With
        {
            .DisplayMode = IconTextRelation.SmallIconUnlabeled,
            .SmallIcon = GetTextControlSmallIcon(identifier, dpi)
        }

        AddHandler bt_control.Click, clickHandler

        Return bt_control
    End Function


    ''' <summary>
    ''' Erstellt einen RibbonButton (Small) <br/>
    ''' Lädt nur TextControl Icons 
    ''' </summary>
    ''' <param name="buttonText">Anzeigetext</param>
    ''' <param name="identifier">Bezeichner für die Icon-Resource von Textcontrol</param>
    ''' <param name="dpi">Dots per Inch (Punktdichte)</param>
    ''' <param name="clickHandler">Handler für das Klick-Event</param>
    ''' <returns></returns>
    Public Function CreateRibbonButtonLarge(buttonText As String, identifier As String, dpi As Integer, clickHandler As EventHandler) As RibbonButton

        Dim bt_control = New RibbonButton() With
        {
            .Text = buttonText,
            .DisplayMode = IconTextRelation.LargeIconLabeled,
            .LargeIcon = GetTextControlLargeIcon(identifier, dpi)
        }

        AddHandler bt_control.Click, clickHandler

        Return bt_control
    End Function

    ''' <summary>
    ''' Erstellt einen RibbonButton ohne Icon
    ''' </summary>
    ''' <param name="buttonText">Anzeigetext</param>
    ''' <param name="clickHandler">Handler für das Klick-Event</param>
    ''' <returns></returns>
    Public Function CreateRibbonButtonNonIconLabeld(buttonText As String, clickHandler As EventHandler) As RibbonButton
        Dim bt_control = New RibbonButton() With
        {
            .Text = buttonText,
            .DisplayMode = IconTextRelation.NoIconLabeled
        }

        AddHandler bt_control.Click, clickHandler

        Return bt_control
    End Function



    Public Function CreateToggleButtonWithDefaultIcon(buttonText As String, identifier As String, dpi As Integer, relation As IconTextRelation) As RibbonToggleButton
        Dim bt_control = New RibbonToggleButton() With
        {
            .DisplayMode = relation
        }

        If buttonText IsNot "" Then
            bt_control.Text = buttonText
        End If

        If relation = IconTextRelation.SmallIconLabeled And Not String.IsNullOrEmpty(identifier) Then
            bt_control.SmallIcon = GetTextControlSmallIcon(identifier, dpi)
        ElseIf relation = IconTextRelation.LargeIconLabeled And Not String.IsNullOrEmpty(identifier) Then
            bt_control.LargeIcon = GetTextControlLargeIcon(identifier, dpi)
        End If

        Return bt_control
    End Function

    Public Function CreateToggleButtonWithDefaultChangeIcon(buttonTextOn As String, buttonTextOff As String, identifierOn As String, identifierOff As String, dpi As Integer, relation As IconTextRelation, Optional isOff As Boolean = True) As RibbonToggleButton

        Dim button = New RibbonToggleButton With {
            .DisplayMode = relation
        }

        If Not String.IsNullOrEmpty(buttonTextOn) AndAlso Not String.IsNullOrEmpty(buttonTextOff) Then
            button.Text = If(isOff, buttonTextOff, buttonTextOn)
        End If

        UpdateIcon(button, If(isOff, identifierOff, identifierOn), relation, dpi)

        AddHandler button.CheckedChanged,
        Sub()
            UpdateIcon(button, If(button.Checked, identifierOn, identifierOff), relation, dpi)
        End Sub

        Return button
    End Function


    Private Sub UpdateIcon(button As RibbonToggleButton, identifier As String, relation As IconTextRelation, dpi As Integer)

        If relation = IconTextRelation.SmallIconLabeled Then
            button.SmallIcon = GetTextControlSmallIcon(identifier, dpi)

        ElseIf relation = IconTextRelation.LargeIconLabeled Then
            button.LargeIcon = GetTextControlLargeIcon(identifier, dpi)
        End If

    End Sub
#End Region
End Module