Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.IO
Imports OfficeOpenXml
Imports System.Xml
Imports System.Drawing
Imports OfficeOpenXml.Style

Public Class ExcelExport
    Public Shared Sub CreateXSLT(ByVal dest As String, ByVal softlist As List(Of software), device As Device, SnapshotTimestamp As String)
        ' Examples: https://github.com/JanKallman/EPPlus/blob/master/SampleApp/Sample1.cs
        Using package As ExcelPackage = New ExcelPackage
            Dim worksheet As ExcelWorksheet = package.Workbook.Worksheets.Add("SoftBase Inventory")
            worksheet.Cells(1, 1).Value = "SoftBase Inventory Report"
            worksheet.Cells(2, 1).Value = $"Device name {device.Hostname}"
            worksheet.Cells(3, 1).Value = $"Inventory timestamp {SnapshotTimestamp}"
            worksheet.Cells(4, 1).Value = $"Device Unique Identifier {device.Uuid}"

            worksheet.Cells(6, 1).Value = "Program name"
            worksheet.Cells(6, 2).Value = "Version"
            Using TitleRange = worksheet.Cells(6, 1, 6, 2)
                With TitleRange
                    .Style.Fill.PatternType = ExcelFillStyle.Solid
                    .Style.Font.Color.SetColor(Color.White)
                    .Style.Fill.BackgroundColor.SetColor(Color.Blue)
                End With
            End Using
            Dim linecount As Integer = 7
            For Each s In softlist
                worksheet.Cells(linecount, 1).Value = s.Name
                worksheet.Cells(linecount, 2).Value = s.Version
                linecount += 1
            Next

            Using range = worksheet.Cells(7, 1, softlist.Count + 6, 2)
                With range
                    .Style.Fill.PatternType = ExcelFillStyle.Solid
                    .Style.Numberformat.Format = "@"
                    .Style.Fill.BackgroundColor.SetColor(Color.White)
                    .Style.Font.Color.SetColor(Color.Black)
                    .Style.Border.BorderAround(ExcelBorderStyle.Thin)
                End With
            End Using
            worksheet.Cells.AutoFitColumns(0)
            Dim xlFile = My.Computer.FileSystem.GetFileInfo(dest)
            package.SaveAs(xlFile)
        End Using
    End Sub
End Class
