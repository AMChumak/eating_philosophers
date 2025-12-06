using System;
using System.Text.Json.Serialization;

namespace NetworkContracts;
public class NoticeForksMessage
{
    public int PhilosopherId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MessageType { get; set; } = "NoticeFork";
}

public class ForkPermissionResponse
{
    public int PhilosopherId { get; set; }
    public bool Granted { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MessageType { get; set; } = "ForkPermissionResponse";
}

public class ForkReleasedMessage
{
    public int ForkId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MessageType { get; set; } = "ForkReleased";
}