using System;
using System.Collections.Generic;

namespace SitronicsApi.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string SecondName { get; set; } = null!;

    public byte[] Password { get; set; } = null!;

    public string Login { get; set; } = null!;

    public virtual Admin? Admin { get; set; }

    public virtual Driver? Driver { get; set; }

    public virtual ICollection<Message> MessageIdRecipientNavigations { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageIdSenderNavigations { get; set; } = new List<Message>();
}
