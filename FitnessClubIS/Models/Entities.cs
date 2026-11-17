using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessClubIS.Models;

[Table("account_type")]
public class AccountType
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; }
}

[Table("account")]
public class Account
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("login")] public string Login { get; set; }
    [Column("password_hash")] public string PasswordHash { get; set; }
    [Column("last_name")] public string LastName { get; set; }
    [Column("first_name")] public string FirstName { get; set; }
    [Column("account_type_id")] public int AccountTypeId { get; set; }
    public AccountType AccountType { get; set; }
}

[Table("client")]
public class Client
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("last_name")] public string LastName { get; set; }
    [Column("first_name")] public string FirstName { get; set; }
    [Column("patronymic")] public string Patronymic { get; set; }
    [Column("birth_date")] public DateTime BirthDate { get; set; }
    [Column("phone")] public string Phone { get; set; }
    [Column("email")] public string Email { get; set; }
    [Column("registered_at")] public DateTime RegisteredAt { get; set; }
    public List<ClientMembership> Memberships { get; set; } = new();

    [NotMapped] public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();

    /// <summary>Текст для карточки клиента: действующий абонемент и дата окончания.</summary>
    [NotMapped]
    public string MembershipText
    {
        get
        {
            var today = DateTime.Today;
            var m = Memberships?.Where(x => x.StartDate <= today && x.EndDate >= today)
                                .OrderByDescending(x => x.EndDate).FirstOrDefault();
            return m == null ? "Нет действующего абонемента"
                             : $"{m.MembershipType?.Name} — до {m.EndDate:dd.MM.yyyy}";
        }
    }

    /// <summary>Есть ли у клиента действующий на сегодня абонемент.</summary>
    [NotMapped]
    public bool HasActiveMembership =>
        Memberships != null && Memberships.Any(m => m.StartDate <= DateTime.Today && m.EndDate >= DateTime.Today);
}

[Table("membership_type")]
public class MembershipType
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; }
    [Column("duration_days")] public int DurationDays { get; set; }
    [Column("visits_limit")] public int? VisitsLimit { get; set; }
    [Column("price")] public decimal Price { get; set; }
}

[Table("client_membership")]
public class ClientMembership
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("client_id")] public int ClientId { get; set; }
    [Column("membership_type_id")] public int MembershipTypeId { get; set; }
    [Column("start_date")] public DateTime StartDate { get; set; }
    [Column("end_date")] public DateTime EndDate { get; set; }
    [Column("visits_left")] public int? VisitsLeft { get; set; }
    public Client Client { get; set; }
    public MembershipType MembershipType { get; set; }
    public List<Payment> Payments { get; set; } = new();
}

[Table("payment")]
public class Payment
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("client_membership_id")] public int ClientMembershipId { get; set; }
    [Column("amount")] public decimal Amount { get; set; }
    [Column("paid_at")] public DateTime PaidAt { get; set; }
    [Column("method")] public string Method { get; set; }
    public ClientMembership ClientMembership { get; set; }
}

[Table("workout_type")]
public class WorkoutType
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; }
    [Column("description")] public string Description { get; set; }
}

[Table("trainer")]
public class Trainer
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("last_name")] public string LastName { get; set; }
    [Column("first_name")] public string FirstName { get; set; }
    [Column("patronymic")] public string Patronymic { get; set; }
    [Column("phone")] public string Phone { get; set; }
    [Column("hired_at")] public DateTime HiredAt { get; set; }
    [NotMapped] public string ShortName => $"{LastName} {FirstName?.FirstOrDefault()}.";
}

[Table("training_session")]
public class TrainingSession
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("workout_type_id")] public int WorkoutTypeId { get; set; }
    [Column("trainer_id")] public int TrainerId { get; set; }
    [Column("starts_at")] public DateTime StartsAt { get; set; }
    [Column("duration_min")] public int DurationMin { get; set; }
    [Column("max_participants")] public int MaxParticipants { get; set; }
    public WorkoutType WorkoutType { get; set; }
    public Trainer Trainer { get; set; }
    public List<Booking> Bookings { get; set; } = new();

    [NotMapped] public int FreePlaces => MaxParticipants - (Bookings?.Count ?? 0);
}

[Table("booking")]
public class Booking
{
    [Key, Column("id")] public int Id { get; set; }
    [Column("session_id")] public int SessionId { get; set; }
    [Column("client_id")] public int ClientId { get; set; }
    [Column("attended")] public bool? Attended { get; set; }
}
