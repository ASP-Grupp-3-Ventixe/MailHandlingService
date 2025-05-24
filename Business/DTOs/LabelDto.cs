using System;

namespace MailHandlingServiceProvider.Business.DTOs;

public class LabelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public int EmailCount { get; set; } // count emails associated with this label
}