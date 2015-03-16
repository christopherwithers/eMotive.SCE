using System.Collections.Generic;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Signups;
using ServiceStack;

namespace eMotive.SCE.Services
{
    //request DTO
    [Route("/Interviews")]
    public class Interviews : IReturn<InterviewResponse>
    {
        public int[] GroupIds { get; set; } 
    }
    //ResponseDTO
    public class InterviewResponse
    {
        public bool Success;
        public IEnumerable<Signup> Result { get; set; }
        public ResponseStatus ResponseStatus { get; set; } //Where Exceptions get auto-serialized
    }

    public class InterviewService : Service
    {
        private readonly ISessionManager signupManager;

        public InterviewService(ISessionManager _signupManager)
        {
            signupManager = _signupManager;
        }

        public object Any(Interviews request)
        {
            return new InterviewResponse { Success = true, Result = signupManager.FetchAll() };
        }
    }
    //HttpContext.Current.User.Identity.IsAuthenticated.ToString()


    //Request
    [Route("/Interviews/List/State")]
    public class UserInterviewStates : IReturn<UserInterviewStatesResponse>
    {
        public string Username { get; set; }
    }
    //Response
    public class UserInterviewStatesResponse
    {
        public bool Success;
        public IEnumerable<SignupState> Result { get; set; }
        public ResponseStatus ResponseStatus { get; set; } //Where Exceptions get auto-serialized
    }

    public class InterviewStateService : Service
    {
        private readonly ISessionManager signupManager;

        public InterviewStateService(ISessionManager _signupManager)
        {
            signupManager = _signupManager;
        }

        public object Any(UserInterviewStates request)
        {
            return new UserInterviewStatesResponse { Success = true, Result = signupManager.FetchSignupStates(request.Username) };
        }
    }
}

//FetchSignupStates
