Imports System
Imports System.Collections
Imports System.Drawing
Imports System.IO
Imports System.IO.Compression
Imports System.Text

Module Program

    Private br As BinaryReader
    Private des As String
    Private source As String

    Sub Main(args As String())

        If args.Count = 0 Then
            Console.WriteLine("Tool UnPack - 2CongLC.vn :: 2024")
        Else
            source = args(0)
        End If

        If IO.File.Exists(source) Then
            br = New BinaryReader(File.OpenRead(source))
            If New String(br.ReadChars(8)) <> "VDISK1.1" Then
                Console.WriteLine("Not a Ragnarok Online VDK file.")
            End If

            Dim unknown1 As Integer = br.ReadInt32()
            Dim fileCount As Integer = br.ReadInt32()
            Dim folderCount As Integer = br.ReadInt32()
            Dim tableStart As Integer = br.ReadInt32() + 145
            Dim unknown2 As Integer = br.ReadInt32()

            br.BaseStream.Position = tableStart
            Dim count As Integer = br.ReadInt32()
            Dim subtables As New List(Of TableData)()
            For i As Integer = 0 To count - 1
                subtables.Add(New TableData)
            Next

            For Each fd As TableData In subtables
                br.BaseStream.Position = fd.start
                Subfile(fd.name)
            Next

            Console.WriteLine("unpack done !")
        End If
        Console.ReadLine()
    End Sub

    Public Class TableData
        Public name As String = New String(br.ReadChars(260)).TrimEnd(ChrW(0))
        Public start As Integer = br.ReadInt32()
    End Class

    Sub Subfile(fullName As String)
        Dim isFolder As Byte = br.ReadByte()
        Dim name As String = New String(br.ReadChars(128)).TrimEnd(ChrW(0))
        Dim sizeUncompressed As Integer = br.ReadInt32()
        Dim sizeCompressed As Integer = br.ReadInt32()
        Dim unknown1 As Integer = br.ReadInt32()
        Dim endPos As Integer = br.ReadInt32()

        des = Path.GetDirectoryName(source) & "\" & Path.GetFileNameWithoutExtension(source)
        Directory.CreateDirectory(des & Path.GetDirectoryName(fullName))

        If isFolder = 1 Then
            Throw New Exception("Fuck!")
        End If

        Using fs As FileStream = File.Create(des & fullName)
            If sizeUncompressed = sizeCompressed Then
                Using bw As New BinaryWriter(fs)
                    bw.Write(br.ReadBytes(sizeUncompressed))
                End Using
                Return
            End If
            br.ReadInt16()
            Using ds As New DeflateStream(New MemoryStream(br.ReadBytes(sizeCompressed)), CompressionMode.Decompress)
                ds.CopyTo(fs)
            End Using
        End Using

    End Sub

End Module
