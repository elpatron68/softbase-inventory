Public Class Donate
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        TxText.Text = "Thanks for using SoftBase Inventory!" + Environment.NewLine + Environment.NewLine +
                      "Although this is just a small program, many hours of development have been spent to make it a useful and stable tool." + Environment.NewLine +
                      "If you like it and want to support the further development, feel free to donate a small amout of money to the author." + Environment.NewLine + Environment.NewLine +
                      "Have fun!"
    End Sub

    Private Sub Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=2K5Z6QV5GREA4&source=url")
    End Sub
End Class
