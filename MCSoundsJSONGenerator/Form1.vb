Imports System.IO
Imports System.Text

Public Class Form1

    Private Structure OGGFileInfo
        Public Property filename
            Set(value)
                m_filename = value
            End Set
            Get
                Return m_filename
            End Get
        End Property

        Public Property catName
            Set(value)
                m_CatName = value
            End Set
            Get
                Return m_CatName
            End Get
        End Property

        Public Property FilePath
            Set(value)
                m_path = value
            End Set
            Get
                Return m_path
            End Get
        End Property

        Private m_CatName As String
        Private m_filename As String
        Private m_path As String

        Public Sub New(ByVal filename As String, Optional ByVal catName As String = "", Optional ByVal path As String = "")
            If path = "" Then
                m_path = filename
                m_filename = filename.Remove(".ogg")
            Else
                m_filename = filename
                m_path = path
            End If
            If catName = "" Then
                If m_filename.Length > 5 Then
                    m_CatName = m_filename.Substring(0, 5).Replace(" ", "_")
                Else
                    m_CatName = m_filename.Replace(" ", "_")
                End If
            Else
                m_CatName = catName
            End If
        End Sub
    End Structure

    Private OGGFileList As List(Of OGGFileInfo) = New List(Of OGGFileInfo)
    Private oggFileArray As OGGFileInfo()
    Private AllGroupName As String = ""

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim FileDlg As New FolderBrowserDialog
        Dim FRetVal = FileDlg.ShowDialog
        If FRetVal = Windows.Forms.DialogResult.Cancel Then Exit Sub
        If Directory.Exists(FileDlg.SelectedPath) = False Then Exit Sub
        TextBox1.Text = FileDlg.SelectedPath
        Panel1.Enabled = True
        ListOggFiles()
    End Sub

    Private Sub TextBox1_Keydown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyData = Keys.Enter Then
            If Directory.Exists(TextBox1.Text) = False Then Exit Sub
            ListOggFiles()
        End If
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        If Directory.Exists(TextBox1.Text) = False Then Panel1.Enabled = False And Button4.Enabled = True
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Me.Close()
    End Sub

    Private Sub ListOggFiles()
        Dim path As String = TextBox1.Text
        Dim DirList As String() = Directory.GetFiles(path, "*.ogg")
        For Each element As String In DirList
            Dim filename As String = (New FileInfo(element)).Name
            ListBox1.Items.Add(filename)
            OGGFileList.Add(New OGGFileInfo(filename.Replace(".ogg", ""), , element))
        Next
        oggFileArray = OGGFileList.ToArray()
    End Sub

    Private Sub ListBox1_SelectedIndexAndValueChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged, ListBox1.SelectedValueChanged
        If CheckBox1.Checked Then
            TextBox2.Text = AllGroupName
            Exit Sub
        End If
        TextBox2.Text = oggFileArray(ListBox1.SelectedIndex).catName
        TextBox2.Focus()
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            Label3.Text = "所有组名"
            TextBox2.Text = AllGroupName
        Else
            Label3.Text = "当前组名"
            If ListBox1.SelectedIndex >= 0 Then TextBox2.Text = oggFileArray(ListBox1.SelectedIndex).catName
        End If
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        If CheckBox1.Checked Then
            AllGroupName = TextBox2.Text
            Exit Sub
        End If
        oggFileArray(ListBox1.SelectedIndex).catName = TextBox2.Text
    End Sub

    Private Sub Panel1_EnabledChanged(sender As Object, e As EventArgs) Handles Panel1.EnabledChanged
        If Panel1.Enabled And TextBox3.Text <> "" Then
            Button3.Enabled = True
        ElseIf Panel1.Enabled = True And TextBox3.Text = "" Then
            Button3.Enabled = False
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim sdlg As SaveFileDialog = New SaveFileDialog
        Dim result As DialogResult
        With sdlg
            .Filter = "json文件|*.json|文本文件|*.txt|所有文件|*.*"
            result = .ShowDialog()
            If result = Windows.Forms.DialogResult.Cancel Then Exit Sub
            TextBox3.Text = .FileName
        End With
        Button3.Enabled = True
    End Sub

    Private Class JSONFile
        Public catname As String
        Public catcontent As String

        ''' <summary>
        ''' init a half-json file
        ''' </summary>
        ''' <param name="a">Catagory Name</param>
        ''' <param name="b">Catagory Content</param>
        ''' <remarks></remarks>
        Public Sub New(Optional ByVal a As String = "", Optional ByVal b As String = "")
            catname = a
            catcontent = b
        End Sub
    End Class

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Try

            If CheckBox1.Checked = True Then
                Dim jfile As JSONFile = New JSONFile(TextBox2.Text)
                For i As Integer = 0 To ListBox1.Items.Count - 1
                    jfile.catcontent += "       """ + oggFileArray(i).filename + """," + vbCrLf
                Next
                Dim sw As New StreamWriter(TextBox3.Text, False)
                sw.Write("{" + vbCrLf + "  """ + jfile.catname + """: {" + vbCrLf + "    sounds: [" + vbCrLf + jfile.catcontent + "    ]" + vbCrLf + "  }" + vbCrLf + "}")
                sw.Flush()
            Else
                Dim jfile As JSONFile()

                For i As Integer = 0 To ListBox1.Items.Count - 1
                    Dim tempJFile As New JSONFile(oggFileArray(i).catName, oggFileArray(i).filename)
                    If i = 0 Then
                        ReDim jfile(1)
                        jfile(0) = New JSONFile
                        jfile(1) = New JSONFile
                    End If
                    Dim existitem As Integer = -1
                    For j As Integer = 0 To UBound(jfile)
                        If jfile(j).Equals(Nothing) Then jfile(j) = New JSONFile
                        If jfile(j).catname = tempJFile.catname Then
                            existitem = j
                            Exit For
                        End If
                    Next
                    If existitem >= 0 Then
                        jfile(existitem).catcontent += "," + vbCrLf + "       """ + tempJFile.catcontent + """"
                    Else
                        Dim emptyIndex As Integer = FindFirstEmptyIndex(jfile)
                        If emptyIndex = -1 Then
                            ReDim Preserve jfile(jfile.Length)
                            jfile(UBound(jfile)) = New JSONFile
                            jfile(UBound(jfile)).catname = tempJFile.catname
                            jfile(UBound(jfile)).catcontent = "       """ + tempJFile.catcontent + """"
                        Else
                            jfile(emptyIndex).catname = tempJFile.catname
                            jfile(emptyIndex).catcontent = "       """ + tempJFile.catcontent + """"
                        End If
                    End If
                Next

                Dim beginStr As String = "{"

                For t As Integer = 0 To UBound(jfile)
                    beginStr = beginStr + vbCrLf + "  """ + jfile(t).catname + """: {" + vbCrLf + "    sounds: [" + vbCrLf + jfile(t).catcontent + vbCrLf + "    ]" + vbCrLf + "  }"
                Next

                beginStr += vbCrLf + "}"
                Dim sw As New StreamWriter(TextBox3.Text)
                sw.Write(beginStr)
                sw.Flush()
            End If
            MessageBox.Show("写入文件" + TextBox3.Text + "完成", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "oops,不小心出错了！", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End Try
    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged
        If TextBox3.Text <> "" Then Button3.Enabled = True
    End Sub

    Private Function FindFirstEmptyIndex(ByVal k As JSONFile()) As Integer
        For i As Integer = 0 To k.Length - 1
            If k(i).catname = "" And k(i).catcontent = "" Then Return i
        Next
        Return -1
    End Function

    Private Function FindItemIndex(ByVal content As String) As Integer
        For i As Integer = 0 To oggFileArray.Length - 1
            If content = oggFileArray(i).filename Then Return i
        Next
        Return -1
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Button4.Parent = Me
        Button4.BringToFront()
        Button4.Location = New Point(Button4.Location.X, Button3.Location.Y + Panel1.Location.Y)
    End Sub
End Class

