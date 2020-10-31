Imports System
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports Discord
Imports Discord.WebSocket

Module Module1
	Private _client as DiscordSocketClient
	Private ReadOnly UserActions as Dictionary(of String, Action(Of SocketMessage, DiscordSocketClient)) = New Dictionary(Of String,Action(Of SocketMessage,DiscordSocketClient))()
  Public AdminUsers as Dictionary(of String, String) = New Dictionary(Of String,String)()
  Sub Main()
	  DotNetEnv.Env.Load()
    ReadSettings()
    for Each pair in AdminUsers
      Console.WriteLine($"{pair.Key}#{pair.Value}")
    Next
	  UserActions.Add("ping", AddressOf Commands.Ping)
	  UserActions.Add("help", AddressOf Help)
	  UserActions.Add("eval", AddressOf Eval)
	  UserActions.Add("addamin", AddressOf AddAdmin)
    MainAsync().GetAwaiter().GetResult()
  End Sub

  Private Async Function MainAsync() As Task
	  Dim config As New DiscordSocketConfig With {
		.LogLevel = LogSeverity.Info
	}

	_client = new DiscordSocketClient(config)

	  AddHandler _client.Log, AddressOf Log

	Await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"))
	Await _client.StartAsync()

	AddHandler _client.Ready, AddressOf Ready
	  AddHandler _client.MessageReceived, AddressOf MessageCreated
	  AddHandler _client.MessageDeleted, AddressOf MessageDel
	Await Task.Delay(-1)
  End Function

	Private Function Log(msg as LogMessage) as Task
		Console.WriteLine(msg)
		return Task.CompletedTask
	End Function

	Private Async Function Ready() as Task
		Await _client.SetGameAsync("my dev suffer from using VB", type := ActivityType.Watching)
	End Function

	Private Function MessageCreated(message as SocketMessage) as Task
		Console.WriteLine($"[{Date.Now()}] -> {message.Author.Username}#{message.Author.Discriminator}: {message.Content}")
	  dim regex = new Regex("^!([\w]+)(?:n| )*", RegexOptions.IgnoreCase)
	  dim key = regex.Match(message.Content).Groups(1).Value
	  If UserActions.ContainsKey(key)
			UserActions(key).Invoke(message, _client)
		End If
		return Task.CompletedTask
	End Function

	Private Function MessageDel(message As Cacheable(Of IMessage, UInt64), channel As ISocketMessageChannel) as Task
		If message.HasValue
			Dim msg = message.Value
			Console.WriteLine($"Message from {msg.Author.Username}#{msg.Author.Discriminator} deleted: {msg.Content}")
		End If

		return Task.CompletedTask
	End Function
  Public Sub WriteSettings()
    Try
      Using fs As FileStream = File.Create("settings.cfg")
        Dim final As String = AdminUsers.Aggregate("", Function(current, entry) current + (entry.Key & "=" + entry.Value & vbLf))

        Dim info As Byte() = New UTF8Encoding(True).GetBytes(final)
        fs.Write(info, 0, info.Length)
      End Using
    Catch ex As Exception
      Console.WriteLine(ex.Message)
    End Try
  End Sub

  Private Sub ReadSettings()
    Try
      Using file = New StreamReader("settings.cfg")
        AdminUsers = New Dictionary(Of String, String)()
        Dim ln As String = file.ReadLine()
        While ln IsNot Nothing
          If ln(0) <> "#"c Then
            Dim values = ln.Split("=")
            AdminUsers.Add(values(0), values(1))
          End If
          ln = file.ReadLine()
        End While
      End Using

    Catch e As InvalidCastException
      Console.WriteLine(e.GetBaseException())
    End Try
  End Sub
End Module
