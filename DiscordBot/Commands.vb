Imports Discord
Imports Discord.WebSocket
Imports Microsoft.CodeAnalysis.CSharp.Scripting

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
        await message.ModifyAsync( Sub(msg) msg.Content = Convert.ToString(CSharpScript.EvaluateAsync(m.Content.Substring(6)).Result))
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
