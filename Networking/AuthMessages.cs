namespace SuperShedAdmin.Networking;

public class AuthRequest {

	public virtual string? Username { get; set; }
	public virtual string? Password { get; set; }
	public virtual string? AuthToken { get; set; }

}

public class AuthResponse {

	public virtual bool? Success { get; set; }
	public virtual string? AuthToken { get; set; }

}