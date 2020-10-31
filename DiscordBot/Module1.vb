Imports System
Imports System.Linq.Expressions
Imports Discord
Imports Discord.WebSocket
Imports Microsoft.CodeAnalysis.CSharp.Scripting

Module Module1
	Private _client as DiscordSocketClient
	Private ReadOnly UserActions as Dictionary(of String, Action(Of SocketMessage, DiscordSocketClient)) = New Dictionary(Of String,Action(Of SocketMessage,DiscordSocketClient))()
    Sub Main()
	    DotNetEnv.Env.Load()
	    UserActions.Add("!ping", AddressOf Pong)
	    UserActions.Add("!help", AddressOf Help)
	    UserActions.Add("!eval", AddressOf Eval)
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
		dim key = message.Content.Split(" ")(0)
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
	
	Private Sub Pong(m As SocketMessage, c As DiscordSocketClient)
		m.Channel.SendMessageAsync($"Pong! {c.Latency}ms") 
	End Sub
	
	Private Sub Help(m As SocketMessage, c As DiscordSocketClient)
		Dim embed As Embed = GenerateEmbed(m.Author, "E que fags")
		m.Channel.SendMessageAsync(Nothing, False, embed)
	End Sub
	
	Private Async Sub Eval(m As SocketMessage, c As DiscordSocketClient)
		If(m.Author.Id = "263073389432930314" or m.Author.Id = "385132696135008259")
			dim message = Await m.Channel.SendMessageAsync("Loading...")
			Console.WriteLine("Test")
			Try
				await message.ModifyAsync( Sub(msg) msg.Content = Convert.ToString(CSharpScript.EvaluateAsync(m.Content.Substring(6)).Result))
			Catch ex As Exception 
				message.ModifyAsync( Sub(msg) msg.Content = ex.Message)
			End Try
		End If
	End Sub
	
	Private Sub ChangeMessage(msg as MessageProperties)
		msg.Content = "Test"
	End Sub
End Module
