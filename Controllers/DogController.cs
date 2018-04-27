using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using DogDay.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq.Expressions;


namespace DogDay.Controllers
{
    public class DogController : Controller
    {
        private DogContext _context;

        public DogController(DogContext context)
        {
            _context = context;
        }
        // GET: /Home/
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            ViewBag.RegisterDog = new RegisterViewModel();
            ViewBag.ProfileDog = new DogProfileViewModel();
            ViewBag.LoginDog = new LoginViewModel();
            return View();
        }

        // TEST ROUTE LANDING_PAGE
        [HttpGet]
        [Route("LANDING_PAGE")]
        public IActionResult LANDING_PAGE()
        {
            int? loggedInId = HttpContext.Session.GetInt32("CurrentDog");
            if (loggedInId == null){
                return RedirectToAction("Index");;
            }
            return View();
        }

        [HttpGet]
        [Route("Search")]
        public IActionResult Search()
        {
            int? dogId = HttpContext.Session.GetInt32("CurrentDog");
            if (dogId == null){
                return RedirectToAction("Index");
            }
            Dog CurrentDog = _context.dogs.Include(d => d.Preferences).ThenInclude(p => p.Filter).Include(d => d.BlockedDogs).Include(d => d.BlockingMe).SingleOrDefault(dog => dog.DogId == dogId);
            IEnumerable<Dog> Dogs = _context.dogs.Include(d => d.Interests).ThenInclude(di => di.Interest).Include(d => d.Humans).ThenInclude(f => f.Human).Include(d => d.Animals).ThenInclude(c => c.Animal).Where(d => d.DogId != dogId).ToList();
            List<Filter> SearchFilters = HttpContext.Session.GetObjectFromJson<List<Filter>>("SearchFilters");
            if(SearchFilters != null){
                foreach (var filter in SearchFilters){
                    if (filter.Category == "Age"){
                        if (filter.LinqFilter == "Puppy"){
                            Dogs = from dog in Dogs
                            where dog.Age <= 3
                            select dog;
                        }
                        if (filter.LinqFilter == "Adult"){
                            Dogs = from dog in Dogs
                            where dog.Age > 3 && dog.Age < 10
                            select dog;
                        }
                        if (filter.LinqFilter == "Senior"){
                            Dogs = from dog in Dogs
                            where dog.Age >= 10
                            select dog;
                        }
                    } else if (filter.Category == "Breed"){
                        Dogs = from dog in Dogs
                            where dog.Breed == filter.UserFilter
                            select dog;
                    } else if (filter.Category == "BodyType"){
                        Dogs = from dog in Dogs
                            where dog.BodyType == filter.UserFilter
                            select dog;
                    } else if (filter.Category == "HighestEducation"){
                        Dogs = from dog in Dogs
                            where dog.HighestEducation == filter.UserFilter
                            select dog;
                    } else if (filter.Category == "Barking"){
                        Dogs = from dog in Dogs
                            where dog.Barking == filter.UserFilter
                            select dog;
                    } else if (filter.Category == "Accidents"){
                        Dogs = from dog in Dogs
                            where dog.Accidents == filter.UserFilter
                            select dog;
                    }
                }
                HttpContext.Session.SetObjectAsJson("SearchFilters", null);
            }
            foreach (var dog in Dogs){
                    dog.MatchPercent = CalculateMatch(dog, CurrentDog);
                }
            Dogs = RemoveBlockages(Dogs, CurrentDog);
            SearchWrapper SearchResults = new SearchWrapper(Dogs, SearchFilters);
            ViewBag.currDogId = dogId;
            return View(SearchResults);
        }

        [HttpPost]
        [Route("ProcessSearch")]
        public IActionResult ProcessSearch(int PrefAge, int PrefBreed, int PrefBodyType, int PrefEducation, int PrefAccidents, int PrefBarking){
            int? dogId = HttpContext.Session.GetInt32("CurrentDog");
            if (dogId == null){
                return RedirectToAction("Index");
            }
            Dog CurrentDog = _context.dogs.Include(d=> d.Preferences).ThenInclude(p => p.Filter).SingleOrDefault(dog => dog.DogId == dogId);
            List<Filter> AllSearchFilters = new List<Filter>();
            if (PrefAge > 0)
                {
                    Filter ageFilter = _context.Filters.SingleOrDefault(f => f.FilterId == PrefAge);
                    if(ageFilter != null){
                        AllSearchFilters.Add(ageFilter);
                    }
                }
            if (PrefBreed > 0)
                {
                    Filter breedFilter = _context.Filters.SingleOrDefault(f => f.FilterId == PrefBreed);
                    if(breedFilter != null){
                        AllSearchFilters.Add(breedFilter);
                    }
                }
            if (PrefBodyType > 0)
                {
                    Filter bodyFilter = _context.Filters.SingleOrDefault(f => f.FilterId == PrefBodyType);
                    if(bodyFilter != null){
                        AllSearchFilters.Add(bodyFilter);
                    }
                }
            if (PrefEducation > 0)
                {
                    Filter educFilter = _context.Filters.SingleOrDefault(f => f.FilterId == PrefEducation);
                    if(educFilter != null){
                        AllSearchFilters.Add(educFilter);
                    }
                }
            if (PrefAccidents > 0)
                {
                    Filter accidentFilter = _context.Filters.SingleOrDefault(f => f.FilterId == PrefAccidents);
                    if(accidentFilter != null){
                        AllSearchFilters.Add(accidentFilter);
                    }
                }
            if (PrefBarking > 0)
                {
                    Filter barkFilter = _context.Filters.SingleOrDefault(f => f.FilterId == PrefBarking);
                    if(barkFilter != null){
                        AllSearchFilters.Add(barkFilter);
                    }
                }
            HttpContext.Session.SetObjectAsJson("SearchFilters", AllSearchFilters);
            return RedirectToAction("Search");
        }

        [HttpGet]
        [Route("Match/{MatchMethod}")]
        public IActionResult Match(string MatchMethod)
        {
            int? dogId = HttpContext.Session.GetInt32("CurrentDog");
            if (dogId == null){
                return RedirectToAction("Index");
            }
            Dog CurrentDog = _context.dogs.Include(d => d.Preferences).ThenInclude(p => p.Filter).SingleOrDefault(dog => dog.DogId == dogId);
            IEnumerable<Dog> Dogs = _context.dogs.Include(d => d.Interests).ThenInclude(di => di.Interest).Include(d => d.Humans).ThenInclude(f => f.Human).Include(d => d.Animals).ThenInclude(c => c.Animal).Where(d => d.DogId != dogId).ToList();
            foreach (var dog in Dogs){
                dog.MatchPercent = CalculateMatch(dog, CurrentDog);
                dog.ReverseMatchPercent = CalculateMatch(CurrentDog, dog);
            }
            List<Filter> SearchFilters = new List<Filter>();
            if (MatchMethod == "Match"){
                Dogs = Dogs.Where(d => d.MatchPercent >= 0.66666);
            } else if (MatchMethod == "Reverse"){
                Dogs = Dogs.Where(d => d.ReverseMatchPercent >= 0.66666);
            } else if (MatchMethod == "Mutual"){
                Dogs = Dogs.Where(d => d.ReverseMatchPercent >= 0.66666 && d.MatchPercent >= 0.66666);
            }
            Dogs = Dogs.OrderByDescending(d => d.MatchPercent);
            Dogs = RemoveBlockages(Dogs, CurrentDog);
            SearchWrapper SearchResults = new SearchWrapper(Dogs, SearchFilters);
            ViewBag.currDogId = dogId;
            return View("Search", SearchResults);
        }


        [HttpGet]
        [Route("UserProfile")]
        public IActionResult Profile()
        {
            int? dogId = HttpContext.Session.GetInt32("CurrentDog");
            if (dogId == null){
                return RedirectToAction("Index");
            }
            Dog CurrentDog = _context.dogs.Include(d => d.Interests).ThenInclude(di => di.Interest).Include(d => d.Humans).ThenInclude(f => f.Human).Include(d => d.Animals).ThenInclude(c => c.Animal).SingleOrDefault(dog => dog.DogId == dogId);
            return View(CurrentDog);
        }

        [HttpPost]
        [Route("PreRegister")]
        public JsonResult PreRegister(RegisterViewModel RegAuth)
        {
            Dictionary<string, string> response = new Dictionary<string, string>();
            response.Add("errors", "True");
            response.Add("Name", "");
            response.Add("Email", "");
            response.Add("Password", "");
            response.Add("PassConf", "");
            if (ModelState.IsValid)
            {
                Dog CurrentDog = _context.dogs.Where(dogs => dogs.Email == RegAuth.Email).SingleOrDefault();
                if(CurrentDog != null)
                {
                    response["Email"] = "That email already exists";
                } else {
                    Dog NewDog = new Dog
                    {
                        Name = RegAuth.Name,
                        Email = RegAuth.Email,
                        Password = RegAuth.Password,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now
                    };
                    PasswordHasher<Dog> Hasher = new PasswordHasher<Dog>();
                    NewDog.Password = Hasher.HashPassword(NewDog, NewDog.Password);
                    ViewBag.ProfileDog = NewDog;
                    _context.dogs.Add(NewDog);
                    _context.SaveChanges();
                    CurrentDog = _context.dogs.SingleOrDefault(dog => dog.Name == NewDog.Name);
                    HttpContext.Session.SetInt32("CurrentDog", CurrentDog.DogId);
                    response["errors"] = "False";
                }
            } 
            else {
                foreach (string key in ViewData.ModelState.Keys){
                    try {
                        string error = ViewData.ModelState.Keys.Where(k => k == key).Select(k => ModelState[k].Errors[0].ErrorMessage).First();
                        response[key] = error;
                    } catch {
                        continue;
                    }
                }
            }
            return Json(response);
        }

        [HttpPost]
        [Route("UpdateProfile")]
        public JsonResult UpdateProfile(DogProfileViewModel DogModel)
        {
            int? dogId = HttpContext.Session.GetInt32("CurrentDog");
            
            Dog CurrentDog = _context.dogs.Include(d => d.Interests)
            .ThenInclude(di => di.Interest)
            .Include(d => d.Humans)
            .ThenInclude(f => f.Human)
            .Include(d => d.Animals)
            .ThenInclude(c => c.Animal).SingleOrDefault(dog => dog.DogId == dogId);
            CurrentDog.Age = DogModel.Age;
            CurrentDog.BodyType = DogModel.BodyType;
            CurrentDog.HighestEducation = DogModel.HighestEducation;
            CurrentDog.Barking = DogModel.Barking;
            CurrentDog.Accidents = DogModel.Accidents;
            CurrentDog.Breed = DogModel.Breed;
            CurrentDog.Description = DogModel.Description;
            foreach(var animal in DogModel.Animals){
                if(CurrentDog.Animals.Where(a => a.Animal_Id == animal).ToList().Count == 0){
                    Cohab newCohab = new Cohab();
                    newCohab.DogId = CurrentDog.DogId;
                    newCohab.Animal_Id = animal;
                    CurrentDog.Animals.Add(newCohab);
                    _context.Cohabs.Add(newCohab);
                }
            }
            foreach(var human in DogModel.Humans){
                if(CurrentDog.Humans.Where(h => h.HumanId == human).ToList().Count == 0){
                    Family newFamily = new Family();
                    newFamily.DogId = CurrentDog.DogId;
                    newFamily.HumanId = human;
                    CurrentDog.Humans.Add(newFamily);
                    _context.Families.Add(newFamily);
                }
            }
            foreach(var interest in DogModel.Interests){
                if(CurrentDog.Interests.Where(i => i.InterestId == interest).ToList().Count == 0){
                    DogInterest newInterest = new DogInterest();
                    newInterest.DogId = CurrentDog.DogId;
                    newInterest.InterestId = interest;
                    CurrentDog.Interests.Add(newInterest);
                    _context.DogInterests.Add(newInterest);
                }
            }
            _context.SaveChanges();
            return Json(CurrentDog);
        }

        [HttpPost]
        [Route("ProcessLogin")]
        public JsonResult ProcessLogin(LoginViewModel LoginAuth)
        {
            Dictionary<string, string> response = new Dictionary<string, string>();
            response.Add("errors", "True");
            response.Add("Email", "");
            response.Add("Password", "");
            if (ModelState.IsValid)
            {
                Dog CurrentDog = _context.dogs.SingleOrDefault(dog => dog.Email == LoginAuth.Email);
                if(CurrentDog != null && LoginAuth.Password != null)
                {
                    var Hasher = new PasswordHasher<Dog>();
                    if(0 != Hasher.VerifyHashedPassword(CurrentDog, CurrentDog.Password, LoginAuth.Password)){
                        HttpContext.Session.SetInt32("CurrentDog", (int)CurrentDog.DogId);
                        response["errors"] = "False";
                    } else {
                        response["Password"] = "Incorrect Password";
                    }
                }
                else
                {
                    response["Email"] = "Does not match our records";
                }
            }
            else
            {
                foreach (string key in ViewData.ModelState.Keys){
                    try {
                        string error = ViewData.ModelState.Keys.Where(k => k == key).Select(k => ModelState[k].Errors[0].ErrorMessage).First();
                        response[key] = error;
                    } catch {
                        continue;
                    }
                }
            }
            return Json(response);
        }

        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout(){
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        
        [Route("Messages")]
        public IActionResult Messages(){
            int? dogId = HttpContext.Session.GetInt32("CurrentDog");
            if (dogId == null){
                return RedirectToAction("Index");
            }
            Dog CurrentDog = _context.dogs.Include(d => d.BlockedDogs).SingleOrDefault(dog => dog.DogId == dogId);
            List<Message> Messages = _context.Messages.Include(m=>m.Sender).OrderByDescending(t => t.created_at).Where(message => message.ReceiverId == dogId).ToList();
            List<Message> Sent = _context.Messages.Include(m=>m.Receiver).OrderByDescending(t => t.created_at).Where(message => message.SenderId == dogId).ToList();
            Messages = RemoveBlockedMessages(Messages, CurrentDog);
            ViewBag.Messages=Messages;
            ViewBag.Sent=Sent;
            ViewBag.currDogId = dogId;
            return View("Messages");
        }
// Profile Stuff**********************************************************************

        [HttpGet]
        [Route("Dashboard")]
        public IActionResult Dashboard(){
            int? dogId = HttpContext.Session.GetInt32("CurrentDog");
            if (dogId == null){
                return RedirectToAction("Index");;
            }
            // Dog CurrentDog = _context.Dogs.Include(d => d.Interests).ThenInclude(di => di.Interest).Include(d => d.Humans).ThenInclude(f => f.Human).Include(d => d.Animals).ThenInclude(c => c.Animal).SingleOrDefault(dog => dog.DogId == dogId);
            // return View(CurrentDog);
            return RedirectToAction("Profile", new { DogId = (int)dogId});
        }

        [HttpGet]
        [Route("Profile/{DogId}")]
        public IActionResult Profile(int DogId){
            int? currDogId = HttpContext.Session.GetInt32("CurrentDog");
            if (currDogId == null){
                return RedirectToAction("Index");;
            }
            ViewBag.currDogId = (int)currDogId;
            Dog ProfileDog = _context.dogs
                    .Include(d => d.Interests)
                    .ThenInclude(di => di.Interest)
                    .Include(d => d.Humans)
                    .ThenInclude(f => f.Human)
                    .Include(d => d.Animals)
                    .ThenInclude(c => c.Animal)
                    .SingleOrDefault(dog => dog.DogId == DogId);
            ViewBag.currDogId = currDogId;
            return View(ProfileDog);
        }

        [HttpPost]
        [Route("PhotoUpload")]
        public async Task<IActionResult> PhotoUpload(IFormFile Photo)
        {
            int? dogId = HttpContext.Session.GetInt32("CurrentDog");
            Dog CurrentDog = _context.dogs.SingleOrDefault(dog => dog.DogId == dogId);
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot/profiles");
            var fileName = CurrentDog.DogId + Path.GetExtension(Photo.FileName);
            CurrentDog.PhotoPath = "/profiles/" + fileName;
            using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create)){
                Console.WriteLine("ready to save");
                await Photo.CopyToAsync(fileStream);
            }
            _context.SaveChanges();
            return RedirectToAction("Profile", new{ DogId = dogId});
        }

        [HttpPost]
        [Route("SetPreferences")]
        public IActionResult SetPreferences(int PrefAge, int PrefBreed, int PrefBodyType, int PrefEducation, int PrefAccidents, int PrefBarking){
            int? dogId = HttpContext.Session.GetInt32("CurrentDog");
            Dog CurrentDog = _context.dogs.Include(d=> d.Preferences).ThenInclude(p => p.Filter).SingleOrDefault(dog => dog.DogId == dogId);
            List<Preference> AllNewPreferences = new List<Preference>();
            if (PrefAge > 0)
                {
                    Preference currPrefAge = CurrentDog.Preferences.SingleOrDefault(p => p.Filter.Category == "Age");
                    if(currPrefAge != null){_context.Preferences.Remove(currPrefAge);}
                    Preference newPreference = new Preference();
                    newPreference.DogId = CurrentDog.DogId;
                    newPreference.FilterId = PrefAge;
                    AllNewPreferences.Add(newPreference);
                }
            if (PrefBreed > 0)
                {
                    Preference currPrefBreed = CurrentDog.Preferences.SingleOrDefault(p => p.Filter.Category == "Breed");
                    if(currPrefBreed != null){_context.Preferences.Remove(currPrefBreed);}
                    Preference newPreference = new Preference();
                    newPreference.DogId = CurrentDog.DogId;
                    newPreference.FilterId = PrefBreed;
                    AllNewPreferences.Add(newPreference);
                }
            if (PrefBodyType > 0)
                {
                    Preference currPrefBodyType = CurrentDog.Preferences.SingleOrDefault(p => p.Filter.Category == "BodyType");
                    if(currPrefBodyType != null){_context.Preferences.Remove(currPrefBodyType);}
                    Preference newPreference = new Preference();
                    newPreference.DogId = CurrentDog.DogId;
                    newPreference.FilterId = PrefBodyType;
                    AllNewPreferences.Add(newPreference);
                }
            if (PrefEducation > 0)
                {
                    Preference currPrefEducation = CurrentDog.Preferences.SingleOrDefault(p => p.Filter.Category == "Education");
                    if(currPrefEducation != null){_context.Preferences.Remove(currPrefEducation);}
                    Preference newPreference = new Preference();
                    newPreference.DogId = CurrentDog.DogId;
                    newPreference.FilterId = PrefEducation;
                    AllNewPreferences.Add(newPreference);
                }
            if (PrefAccidents > 0)
                {
                    Preference currPrefAccidents = CurrentDog.Preferences.SingleOrDefault(p => p.Filter.Category == "Accidents");
                    if(currPrefAccidents != null){_context.Preferences.Remove(currPrefAccidents);}
                    Preference newPreference = new Preference();
                    newPreference.DogId = CurrentDog.DogId;
                    newPreference.FilterId = PrefAccidents;
                    AllNewPreferences.Add(newPreference);
                }
            if (PrefBarking > 0)
                {
                    Preference currPrefBarking = CurrentDog.Preferences.SingleOrDefault(p => p.Filter.Category == "Barking");
                    if(currPrefBarking != null){_context.Preferences.Remove(currPrefBarking);}
                    Preference newPreference = new Preference();
                    newPreference.DogId = CurrentDog.DogId;
                    newPreference.FilterId = PrefBarking;
                    AllNewPreferences.Add(newPreference);
                }
            foreach (Preference preference in AllNewPreferences){
                _context.Add(preference);
            }
            _context.SaveChanges();
            return RedirectToAction("Profile", new{ DogId = dogId});
        }

// MESSAGES ROUTE**********************************************************************

        [HttpPost]
        [Route("sendMessage")]
        public IActionResult PostMessage(int reciever, Message Message)
        {

            int? loggedInId = HttpContext.Session.GetInt32("CurrentDog");
            if (loggedInId == null){
                // WHERE DO YOU WANT TO GO IF NOT LOGGED IN????**********************
            }

            if (ModelState.IsValid)
            {
                Dog CurrentDog = _context.dogs.Where(dogs => dogs.DogId == loggedInId).SingleOrDefault();
                
                Message NewMessage = new Message
                {
                    SenderId = CurrentDog.DogId,
                    MessageContent = Message.MessageContent,
                    ReceiverId = reciever,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                _context.Messages.Add(NewMessage);
                _context.SaveChanges();

            }
            return RedirectToAction("Profile", new { DogId = (int)reciever});
    
        }

        [HttpPost]
        [Route("replyMessage")]
        public IActionResult replyMessage(int reciever, Message Message)
        {

            int? loggedInId = HttpContext.Session.GetInt32("CurrentDog");
            if (loggedInId == null){
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                Dog CurrentDog = _context.dogs.Where(dogs => dogs.DogId == loggedInId).SingleOrDefault();
                
                Message NewMessage = new Message
                {
                    SenderId = CurrentDog.DogId,
                    MessageContent = Message.MessageContent,
                    ReceiverId = reciever,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                _context.Messages.Add(NewMessage);
                _context.SaveChanges();

            }
            return RedirectToAction("Messages");
    
        }
// Extra Stuff**********************************************************************

        [HttpGet]
        [Route("Block/{DogId}")]
        public IActionResult Block(int DogId){
            int? loggedInId = HttpContext.Session.GetInt32("CurrentDog");
            Block newBlocking = new Block();
            newBlocking.DogBlockingId = (int)loggedInId;
            newBlocking.BlockedDogId = DogId;
            _context.Blocks.Add(newBlocking);
            _context.SaveChanges();
            return RedirectToAction("Search");
        }

        public IEnumerable<Dog> RemoveBlockages (IEnumerable<Dog> Dogs, Dog CurrentDog){
            List<Dog> DogsToRemove = new List<Dog>();
            foreach (var blockage in CurrentDog.BlockedDogs){
                Dog toRemove = Dogs.SingleOrDefault(d => d.DogId == blockage.BlockedDogId);
                DogsToRemove.Add(toRemove);
            }
            foreach (var blockage in CurrentDog.BlockingMe){
                Dog toRemove = Dogs.SingleOrDefault(d => d.DogId == blockage.DogBlockingId);
                DogsToRemove.Add(toRemove);
            }
            Dogs = Dogs.Except(DogsToRemove);
            return Dogs;
        }

        public List<Message> RemoveBlockedMessages(List<Message> Messages, Dog CurrentDog){
            foreach (var blockage in CurrentDog.BlockedDogs){
                Message toRemove = Messages.SingleOrDefault(m => m.SenderId == blockage.BlockedDogId);
                Messages.Remove(toRemove);
            }
            return Messages;
        }
        public double CalculateMatch(Dog dMatch, Dog dLoggedIn){
            // Calculate Total Possible Points
            double possiblePoints = 1.0;
            double matchPoints = 0.0;
            if (dLoggedIn.Age > 0){
                possiblePoints +=1.0;
                if (Math.Abs(dMatch.Age - dLoggedIn.Age)<2){
                    matchPoints += 1.0;
                }
            }
            if (dLoggedIn.Breed != null){
                possiblePoints +=1.0;
                if (dMatch.Breed == dLoggedIn.Breed){
                    matchPoints += 1.0;
                }
            }
            if (dLoggedIn.BodyType != null){
                possiblePoints +=1.0;
                if (dMatch.BodyType == dLoggedIn.BodyType){
                    matchPoints += 1.0;
                }
            }
            if (dLoggedIn.HighestEducation != null){
                possiblePoints +=1;
                if (dMatch.HighestEducation == dLoggedIn.HighestEducation){
                    matchPoints += 1;
                }
            }
            if (dLoggedIn.Barking != null){
                possiblePoints +=1.0;
                if (dMatch.Barking == dLoggedIn.Barking){
                    matchPoints += 1.0;
                }
            }
            if (dLoggedIn.Accidents != null){
                possiblePoints +=1.0;
                if (dMatch.Accidents == dLoggedIn.Accidents){
                    matchPoints += 1.0;
                }
            }
            if (dLoggedIn.Interests != null){
                possiblePoints += 1.0;
                foreach (var DLIinterest in dLoggedIn.Interests){
                    if (dMatch.Interests.Where(i => i.InterestId == DLIinterest.InterestId).ToList().Count != 0){
                        matchPoints += 1.0;
                    }
                }
            }
            if (dLoggedIn.Humans != null){
                possiblePoints += 1.0;
                foreach (var DLIhuman in dLoggedIn.Humans){
                    if (dMatch.Humans.Where(h => h.HumanId == DLIhuman.HumanId).ToList().Count != 0){
                        matchPoints += 1.0;
                    }
                }
            }
            if (dLoggedIn.Animals != null){
                possiblePoints += 1.0;
                foreach (var DLIanimal in dLoggedIn.Animals){
                    if (dMatch.Animals.Where(a => a.Animal_Id == DLIanimal.Animal_Id).ToList().Count != 0){
                        matchPoints += 1.0;
                    }
                }
            }
            if (dLoggedIn.Preferences != null){
                possiblePoints += dLoggedIn.Preferences.Count();
                foreach (var preference in dLoggedIn.Preferences){
                    Filter filter = preference.Filter;
                    if (filter.Category == "Age"){
                        if (filter.LinqFilter == "Puppy"){
                            if(dMatch.Age <= 3){
                                matchPoints += 1.0;
                            }
                        }
                        if (filter.LinqFilter == "Adult"){
                            if(dMatch.Age > 3 && dMatch.Age <10){
                                matchPoints += 1.0;
                            }
                        }
                        if (filter.LinqFilter == "Senior"){
                            if(dMatch.Age >= 10){
                                matchPoints += 1.0;
                            }
                        }
                    } else if (filter.Category == "Breed"){
                            if(dMatch.Breed == filter.UserFilter){
                                matchPoints += 1.0;
                            }
                    } else if (filter.Category == "BodyType"){
                            if(dMatch.BodyType == filter.UserFilter){
                                matchPoints += 1.0;
                            }
                    } else if (filter.Category == "HighestEducation"){
                            if(dMatch.HighestEducation == filter.UserFilter){
                                matchPoints += 1.0;
                            }
                    } else if (filter.Category == "Barking"){
                            if(dMatch.Barking == filter.UserFilter){
                                matchPoints += 1.0;
                            }
                    } else if (filter.Category == "Accidents"){
                            if(dMatch.Accidents == filter.UserFilter){
                                matchPoints += 1.0;
                            }
                    }
                }
            }
            double matchRatio = matchPoints/(possiblePoints/3.0);
            if(matchRatio > 1.0){
                matchRatio = 0.99;
            } else if (matchRatio < 0.009){
                matchRatio = 0.01;
            }
            return matchRatio;
        }
    }
}