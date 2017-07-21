using Collectively.Common.Security;
using Collectively.Messages.Commands;
using Collectively.Messages.Commands.Users;
using Collectively.Services.Users.Services;
using Nancy;

namespace Collectively.Services.Users.Modules
{
    public class AuthenticationModule : ModuleBase
    {
        public AuthenticationModule(IServiceAuthenticatorHost serviceAuthenticatorHost,
            IAuthenticationService authenticationService,
            IUserService userService,
            IJwtTokenHandler jwtTokenHandler, 
            ICommandHandler<SignIn> signInHandler) 
            : base(requireAuthentication: false)
        {
            Post("authenticate", args => 
            {
                var credentials = BindRequest<Credentials>();
                var token = serviceAuthenticatorHost.CreateToken(credentials);
                if (token.HasNoValue)
                {
                    return HttpStatusCode.Unauthorized;
                }
                
                return token.Value;
            });

            Post("sign-in", async args =>
            {
                var credentials = BindRequest<SignIn>();
                await signInHandler.HandleAsync(credentials);
                var session = await authenticationService.GetSessionAsync(credentials.SessionId);
                if (session.HasNoValue)
                {
                    return HttpStatusCode.Unauthorized;
                }
                var user = await userService.GetAsync(session.Value.UserId);
                var token = jwtTokenHandler.Create(user.Value.UserId, user.Value.Role);

                return token.Value;
            });
        }        
    }
}