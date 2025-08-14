﻿namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// The event that is invoked when the voting phase of a meeting is complete.
/// </summary>
/// <param name="meetingHud">The MeetingHud instance.</param>
public class VotingCompleteEvent(MeetingHud meetingHud) : MiraEvent
{
    /// <summary>
    /// Gets the MeetingHud instance.
    /// </summary>
    public MeetingHud MeetingHud { get; } = meetingHud;
}
