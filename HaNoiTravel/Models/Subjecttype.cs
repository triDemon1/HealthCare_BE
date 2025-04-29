using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Subjecttype
{
    public int Typeid { get; set; }

    public string Subjectname { get; set; } = null!;

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
