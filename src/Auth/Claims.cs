namespace datotekica.Auth;

///<Summary>
/// Claim names from <a href="https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims">OpenId Connect specification</a>.
///</Summary>
public struct Claims
{
    /// <summary>
    /// Identifier for the End-User at the Issuer.
    /// </summary>
    public const string Subject = "sub";
    /// <summary>
    /// End-User's full name in displayable form including all name parts, possibly including titles and suffixes, ordered according to the End-User's locale and preferences. 
    /// </summary>
    public const string DisplayName = "name";
    /// <summary>
    /// Given name(s) or first name(s) of the End-User. Note that in some cultures, people can have multiple given names; all can be present, with the names being separated by space characters. 
    /// </summary>
    public const string FirstName = "given_name";
    /// <summary>
    /// Surname(s) or last name(s) of the End-User. Note that in some cultures, people can have multiple family names or no family name; all can be present, with the names being separated by space characters. 
    /// </summary>
    public const string LastName = "family_name";
    /// <summary>
    /// Casual name of the End-User that may or may not be the same as the given_name. For instance, a nickname value of Mike might be returned alongside a given_name value of Michael.
    /// </summary>
    public const string NickName = "nickname";
    /// <summary>
    /// Shorthand name by which the End-User wishes to be referred to at the RP, such as janedoe or j.doe. This value MAY be any valid JSON string including special characters such as @, /, or whitespace. The RP MUST NOT rely upon this value being unique.
    /// </summary>
    public const string UserName = "preferred_username";
    /// <summary>
    /// URL of the End-User's profile page. The contents of this Web page SHOULD be about the End-User. 
    /// </summary>
    public const string Profile = "profile";
    /// <summary>
    /// URL of the End-User's profile picture. This URL MUST refer to an image file (for example, a PNG, JPEG, or GIF image file), rather than to a Web page containing an image. Note that this URL SHOULD specifically reference a profile photo of the End-User suitable for displaying when describing the End-User, rather than an arbitrary photo taken by the End-User.
    /// </summary>
    public const string Picture = "picture";
    /// <summary>
    /// URL of the End-User's Web page or blog. This Web page SHOULD contain information published by the End-User or an organization that the End-User is affiliated with.
    /// </summary>
    public const string Website = "website";
    /// <summary>
    /// End-User's preferred e-mail address. The RP MUST NOT rely upon this value being unique.
    /// </summary>
    public const string Email = "email";
    /// <summary>
    /// True if the End-User's e-mail address has been verified; otherwise false. When this Claim Value is true, this means that the OP took affirmative steps to ensure that this e-mail address was controlled by the End-User at the time the verification was performed. The means by which an e-mail address is verified is context-specific, and dependent upon the trust framework or contractual agreements within which the parties are operating.
    /// </summary>
    public const string EmailVerified = "email_verified";
    /// <summary>
    /// End-User's gender. Values defined by this specification are female and male. Other values MAY be used when neither of the defined values are applicable.
    /// </summary>
    public const string Gender = "gender";
    /// <summary>
    /// End-User's birthday, represented as an ISO8601‑2004 YYYY-MM-DD format. The year MAY be 0000, indicating that it is omitted. To represent only the year, YYYY format is allowed. Note that depending on the underlying platform's date related function, providing just year can result in varying month and day, so the implementers need to take this factor into account to correctly process the dates.
    /// </summary>
    public const string BirthDate = "birthdate";
    /// <summary>
    /// String from time zone database representing the End-User's time zone. For example, Europe/Paris or America/Los_Angeles.
    /// </summary>
    public const string TimeZone = "zoneinfo";
    /// <summary>
    /// End-User's locale, represented as a RFC5646 language tag. This is typically an ISO639‑1 language code in lowercase and an ISO3166‑1 country code in uppercase, separated by a dash. For example, en-US or fr-CA. As a compatibility note, some implementations have used an underscore as the separator rather than a dash, for example, en_US; Relying Parties MAY choose to accept this locale syntax as well.
    /// </summary>
    public const string Locale = "locale";
    /// <summary>
    /// End-User's preferred telephone number. E.164 is RECOMMENDED as the format of this Claim, for example, +1 (425) 555-1212 or +56 (2) 687 2400. If the phone number contains an extension, it is RECOMMENDED that the extension be represented using the RFC3966 extension syntax, for example, +1 (604) 555-1234;ext=5678. 
    /// </summary>
    public const string PhoneNumber = "phone_number";
    /// <summary>
    /// True if the End-User's phone number has been verified; otherwise false. When this Claim Value is true, this means that the OP took affirmative steps to ensure that this phone number was controlled by the End-User at the time the verification was performed. The means by which a phone number is verified is context-specific, and dependent upon the trust framework or contractual agreements within which the parties are operating. When true, the phone_number Claim MUST be in E.164 format and any extensions MUST be represented in RFC 3966 format. 
    /// </summary>
    public const string PhoneNumberVerified = "phone_number_verified";
    /// <summary>
    /// Time the End-User's information was last updated. Its value is a JSON number representing the number of seconds from 1970-01-01T0:0:0Z as measured in UTC until the date/time. 
    /// </summary>
    public const string UpdatedAt = "updated_at";
}