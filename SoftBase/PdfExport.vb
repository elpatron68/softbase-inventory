Imports System
Imports System.IO
Imports iText.IO.Font.Constants
Imports iText.Kernel.Font
Imports iText.Kernel.Pdf
Imports iText.Layout
Imports iText.Layout.Element
Imports iText.Test.Attributes

Public Class PdfExport

    Public Shared Sub CreatePdf(ByVal dest As String, ByVal softlist As List(Of software))

        Dim writer As PdfWriter = New PdfWriter(dest)
        Dim pdf As PdfDocument = New PdfDocument(writer)
        Dim document As Document = New Document(pdf)
        Dim font As PdfFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN)
        document.Add(New Paragraph("iText is:").SetFont(font))
        Dim list As List = New List().SetSymbolIndent(12).SetListSymbol("•").SetFont(font)
        For Each soft In softlist
            list.Add($"{soft.Name} - Version {soft.Version}")
        Next
        'list.Add(New ListItem("Never gonna give you up")).Add(New ListItem("Never gonna let you down")).Add(New ListItem("Never gonna run around and desert you")).Add(New ListItem("Never gonna make you cry")).Add(New ListItem("Never gonna say goodbye")).Add(New ListItem("Never gonna tell a lie and hurt you"))
        document.Add(list)
        document.Close()
    End Sub
End Class
