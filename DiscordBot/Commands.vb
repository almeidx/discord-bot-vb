Imports System.IO
Imports Discord
Imports Discord.WebSocket
Imports Microsoft.CodeAnalysis.CSharp.Scripting
Imports System.Math
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports Microsoft.CodeAnalysis.Scripting

Public Module Commands
  Public Sub Ping(m As SocketMessage, c As DiscordSocketClient)
    m.Channel.SendMessageAsync($"Pong! {c.Latency}ms")
  End Sub

  Public Sub Help(m As SocketMessage, c As DiscordSocketClient)
    Dim embed As Embed = GenerateEmbed(m.Author, "E que fags")
    m.Channel.SendMessageAsync(Nothing, False, embed)
  End Sub

  Public Async Sub Eval(m As SocketMessage, c As DiscordSocketClient)
    If(AdminUsers.Values.Contains(m.Author.Id))
      dim message = Await m.Channel.SendMessageAsync("Loading...")
      Try
        dim command = m.Content.Substring(6)
        Console.WriteLine(command)
        dim _imports as List (Of String) = New List(Of String)()
        dim newCommand as String = ""
        Dim reader = new StringReader(command)
        While True
          Dim line = reader.ReadLine()
          If line is Nothing
            Exit While
          Else
            if line.Contains("using")
              dim l = line
              l = l.Replace("using ", "")
              l = l.Replace(";", "")
              _imports.Add(l)
            Else
              newCommand += line
            End If
          End If
        End While
        await message.ModifyAsync( Sub(msg) msg.Content = Convert.ToString(CSharpScript.EvaluateAsync(newCommand ,ScriptOptions.Default.WithImports(_imports.ToArray())).Result))
      Catch ex As Exception
        message.ModifyAsync( Sub(msg) msg.Content = ex.Message)

      End Try
    Else
      Await m.Channel.SendMessageAsync("User is not allowed to run this command.")
    End If
  End Sub

  Public Sub AddAdmin(m As SocketMessage, c As DiscordSocketClient)
    If(AdminUsers.Values.Contains(m.Author.Id))
      If not AdminUsers.ContainsValue(m.Content.Split(" ")(2)) and not AdminUsers.ContainsKey(m.Content.Split(" ")(1))
        AdminUsers.Add(m.Content.Split(" ")(1),m.Content.Split(" ")(2))
        WriteSettings()
        m.Channel.SendMessageAsync("User is now an admin.")
      Else
        m.Channel.SendMessageAsync("User is already an admin.")
      End If
    Else
      m.Channel.SendMessageAsync("User is not allowed to run this command.")
    End If
  End Sub
  Private Function GenerateEmbed(author As IUser, Optional description As String = Nothing)
    Dim builder = new EmbedBuilder()
    If description <> Nothing And description <> ""
      builder.WithDescription(description)
    End If
    builder.WithFooter($"{author.Username}#{author.Discriminator}")
    builder.WithAuthor($"{author.Username}#{author.Discriminator}", author.GetAvatarUrl())
    builder.WithTimestamp(Date.Now())
    builder.WithColor(Color.Blue)
    return builder.Build()
   End Function
End Module
