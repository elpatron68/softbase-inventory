Imports System
Imports System.IO
Imports iText.IO.Font.Constants
Imports iText.Kernel.Font
Imports iText.Kernel.Pdf
Imports iText.Layout
Imports iText.Layout.Element

Public Class PdfExport

    Public Shared Sub CreatePdf(ByVal dest As String, ByVal softlist As List(Of software))

        Dim writer As PdfWriter = New PdfWriter(dest)
        Dim pdf As PdfDocument = New PdfDocument(writer)
        Dim document As Document = New Document(pdf)
        Dim font As PdfFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD)
        document.Add(New Paragraph("Software inventory for machine [NAME] created at [DATE]:").SetFont(font))
        PdfFontFactory.CreateFont(StandardFonts.COURIER)
        Dim list As List = New List().SetSymbolIndent(12).SetListSymbol("•").SetFont(font)
        For Each soft In softlist
            list.Add($"{soft.Name} - Version {soft.Version}")
        Next
        document.Add(list)
        document.Close()
    End Sub
End Class
