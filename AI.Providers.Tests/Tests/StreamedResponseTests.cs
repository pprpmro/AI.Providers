using System.Net;
using System.Text;
using AI.Providers.Dialogs;
using AI.Providers.Dialogs.Implementations;
using AI.Providers.Tests.Mocks;

namespace AI.Providers.Tests.Tests;

public class StreamedResponseTests
{
	[Fact]
	public async Task ReadsContentThroughKeepAliveAndStopsAtDone()
	{
		using var client = CreateClient(
			": keep-alive\r\n\r\nevent: message\nid: 1\nretry: 1000\nextension: ignored\n" +
			"data: {\"choices\":[{\"delta\":{\"role\":\"assistant\"}}]}\n\n" +
			"data: {\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}\r\n\r\n" +
			": keep-alive\n\ndata:\n\n" +
			"data:{\"choices\":[{\"delta\":{\"content\":\" world\"}}]}\n\n" +
			"data: [DONE]\n\ndata: not-json\n\n");
		using var dialog = new OpenAiCompatibleDialog("model", "https://chat.example.test/completions", client);
		var chunks = new List<string>();

		var result = await dialog.SendMessage(new ChatRequest { Message = "hello", Stream = true }, chunks.Add);

		Assert.Equal("Hello world", result);
		Assert.Equal(new[] { "Hello", " world" }, chunks);
		Assert.Equal(new[] { "hello", "Hello world" }, dialog.GetHistoryCopy().Select(message => message.Content));
	}

	[Fact]
	public async Task InvalidDataAfterPartialResponseFailsWithoutSavingTheTurn()
	{
		using var client = CreateClient(
			"data: {\"choices\":[{\"delta\":{\"content\":\"partial\"}}]}\n\ndata: not-json\n\n");
		using var dialog = new OpenAiCompatibleDialog("model", "https://chat.example.test/completions", client);
		dialog.SetSystemPrompt("system");
		var chunks = new List<string>();

		await Assert.ThrowsAsync<Exception>(() =>
			dialog.SendMessage(new ChatRequest { Message = "hello", Stream = true }, chunks.Add));

		Assert.Equal(new[] { "partial" }, chunks);
		Assert.Equal("system", Assert.Single(dialog.GetHistoryCopy()).Content);
	}

	[Fact]
	public async Task CancellationAfterFirstChunkDoesNotReturnOrRememberPartialSuccess()
	{
		using var client = CreateClient(
			"data: {\"choices\":[{\"delta\":{\"content\":\"first\"}}]}\n\n" +
			"data: {\"choices\":[{\"delta\":{\"content\":\"second\"}}]}\n\ndata: [DONE]\n\n");
		using var dialog = new OpenAiCompatibleDialog("model", "https://chat.example.test/completions", client);
		using var cancellation = new CancellationTokenSource();
		var chunks = new List<string>();

		var send = dialog.SendMessage(new ChatRequest { Message = "hello", Stream = true }, chunk =>
		{
			chunks.Add(chunk);
			cancellation.Cancel();
		}, cancellation.Token);

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => send);
		Assert.True(send.IsCanceled);
		Assert.Equal(new[] { "first" }, chunks);
		Assert.Empty(dialog.GetHistoryCopy());
	}

	[Fact]
	public async Task CancellationInterruptsWaitingForTheNextNetworkChunk()
	{
		using var stream = new PendingReadStream();
		using var client = new HttpClient(new MockHttpMessageHandler((_, _) => Task.FromResult(
			new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) })));
		using var dialog = new OpenAiCompatibleDialog("model", "https://chat.example.test/completions", client);
		using var cancellation = new CancellationTokenSource();

		var send = dialog.SendMessage(new ChatRequest { Message = "hello", Stream = true }, cancellationToken: cancellation.Token);
		await Task.WhenAny(stream.ReadStarted.Task, send).WaitAsync(TimeSpan.FromSeconds(3));
		Assert.True(stream.ReadStarted.Task.IsCompletedSuccessfully);
		Assert.False(send.IsCompleted);
		cancellation.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => send.WaitAsync(TimeSpan.FromSeconds(3)));
		Assert.True(send.IsCanceled);
		Assert.Empty(dialog.GetHistoryCopy());
	}

	private static HttpClient CreateClient(string payload)
		=> new(new MockHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(payload, Encoding.UTF8, "text/event-stream")
		})));

	private sealed class PendingReadStream : Stream
	{
		private readonly TaskCompletionSource<int> pendingRead = new(TaskCreationOptions.RunContinuationsAsynchronously);
		public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

		public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			ReadStarted.TrySetResult();
			return new ValueTask<int>(pendingRead.Task.WaitAsync(cancellationToken));
		}

		public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException("Synchronous network reads are not allowed.");
		public override void Flush() => throw new NotSupportedException();
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

		protected override void Dispose(bool disposing)
		{
			pendingRead.TrySetResult(0);
			base.Dispose(disposing);
		}
	}
}