using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace LyncPortable
{
    public class LyncHttpClientPortable : IDisposable
    {
        HttpClient _httpClient;
        public event Action<string> Notify;

        private void OnNotify(string message)
        {
            if (Notify != null) Notify(message + "\n\n");
        }

        string _rootUri;

        FormUrlEncodedContent userTokenRequestContent = new FormUrlEncodedContent(new[] 
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", @"jack-wolfskin\ID_A_IT_Toilet_Male"),
            new KeyValuePair<string, string>("password", "lynC4toileT!")
        });

        public LyncHttpClientPortable()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Clear();
        }

        private string PresenceLink { get; set; }
        private string ReportMyActivityLink { get; set; }

        private HttpResponseMessage GetAsyncResult(string uri)
        {
            return _httpClient.GetAsync(uri).Result;
        }

        private HttpResponseMessage PostAsyncResult(string uri, HttpContent content)
        {
            return _httpClient.PostAsync(uri, content).Result;
        }

        private void AddToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        }

        private StringContent GetJsonStringContent<T>(T parm) where T : class
        {
            return new StringContent(JsonConvert.SerializeObject(parm), System.Text.Encoding.UTF8, "application/json");
        }

        public void SetStatus(string status)
        {
            var availabilityPostResult = this.PostAsyncResult(PresenceLink, GetJsonStringContent(new Availability(status)));
            OnNotify(availabilityPostResult.Content.ToString());

            if (availabilityPostResult.IsSuccessStatusCode)
            {
                // Status muss abgeholt werden, damit er auch gesetzt wird
                //var availabiltyGetResult = this.GetAsyncResult(PresenceLink);
                //OnNotify(availabiltyGetResult.Content.ReadAsStringAsync().Result);
                GetStatus();
            }
        }

        public string GetStatus()
        {
            var availabilityGetResult = this.GetAsyncResult(PresenceLink);
            var availabilityResult = JsonConvert.DeserializeObject<AvailabilityResult>(availabilityGetResult.Content.ReadAsStringAsync().Result);

            return availabilityResult.availability;
        }

        public void GetEventChannel(string link)
        {
            var result = _httpClient.GetAsync(link).Result;
            var test = result.Content.ReadAsStringAsync();
        }

        public void ReportMyActivity()
        {
            var activityResult = this.PostAsyncResult(ReportMyActivityLink, null);
            OnNotify(activityResult.StatusCode.ToString());
        }


        public void Init()
        {
            string userTokenRequestLink = "";


            //var discoverResult = this.GetAsyncResult("https://lyncdiscoverinternal.jack-wolfskin.com/");

            //if (discoverResult.IsSuccessStatusCode == false)
            //{
                var discoverResult = this.GetAsyncResult("https://lyncdiscover.jack-wolfskin.com/");
            //}

            OnNotify(discoverResult.Content.ReadAsStringAsync().Result);

            if (discoverResult.IsSuccessStatusCode)
            {
                var discover = JsonConvert.DeserializeObject<Discover>(discoverResult.Content.ReadAsStringAsync().Result);

                var rootUri = new Uri(discover._links.user.href);
                _rootUri = rootUri.Scheme + "://" + rootUri.Host;

                var result = this.GetAsyncResult(discover._links.user.href);
                var authenticate = result.Headers.WwwAuthenticate;


                foreach (var item in authenticate)
                {
                    var parameter = item.Parameter;
                    var splitParameter = parameter.Split(new string[] { "\"" }, StringSplitOptions.None);

                    userTokenRequestLink = Array.Find(splitParameter, element => element.StartsWith(_rootUri));
                }

                //userTokenRequestLink = _rootUri + "/WebTicket/oauthtoken";


                var userResult = this.PostAsyncResult(userTokenRequestLink, userTokenRequestContent);
                var userObject = JsonConvert.DeserializeObject<UserToken>(userResult.Content.ReadAsStringAsync().Result);

                OnNotify(userResult.Content.ReadAsStringAsync().Result);

                // Ab sofort alle requests mit Token
                this.AddToken(userObject.access_token);

                // Application URL holen
                var applicationResult = this.GetAsyncResult(discover._links.user.href);
                var applications = JsonConvert.DeserializeObject<Applications>(applicationResult.Content.ReadAsStringAsync().Result);

                OnNotify(applicationResult.Content.ReadAsStringAsync().Result);

                ApplicationPostContent appPostContent = new ApplicationPostContent("Toilet", Guid.NewGuid().ToString(), "de");

                var createApplicationResultJson = this.PostAsyncResult(applications._links.applications.href, GetJsonStringContent(appPostContent));

                if (createApplicationResultJson.IsSuccessStatusCode)
                {
                    var createApplicationResult = JsonConvert.DeserializeObject<CreateApplicationResult>(createApplicationResultJson.Content.ReadAsStringAsync().Result);

                    OnNotify(createApplicationResultJson.Content.ReadAsStringAsync().Result);

                    var makeMeAvailableUri = _rootUri + createApplicationResult._embedded.me._links.makeMeAvailable.href;

                    // wird benötigt, um den Status setzen zu können
                    var makeMeAvailableResult = this.PostAsyncResult(makeMeAvailableUri, GetJsonStringContent(new MakeMeAvailableContent("", AvailabilityStatus.Online)));

                    PresenceLink = _rootUri + createApplicationResult._embedded.me._links.self.href + "/presence";
                    ReportMyActivityLink = _rootUri + createApplicationResult._embedded.me._links.self.href + "/reportMyActivity";
                }
            }
            else
            {
                // hier gehts nicht mehr weiter :(
            }

          
        }

        public void Dispose()
        {

        }

        internal class Discover
        {
            public DiscoverLinks _links { get; set; }
        }

        internal class DiscoverLinks
        {
            public Link self { get; set; }
            public Link user { get; set; }
            public Link xframe { get; set; }
        }

        internal class Link
        {
            public string href { get; set; }
        }

        internal class UserToken
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string ms_rtc_identityscope { get; set; }
            public string token_type { get; set; }
        }

        internal class Applications
        {
            public ApplicationLinks _links { get; set; }
        }

        internal class ApplicationLinks
        {
            public Link self { get; set; }
            public Link applications { get; set; }
            public Link xframe { get; set; }
        }

        internal class ApplicationPostContent
        {
            public ApplicationPostContent(string userAgent, string endpointId, string culture)
            {
                UserAgent = userAgent;
                EndpointId = endpointId;
                Culture = culture;
            }

            public string UserAgent { get; set; }
            public string EndpointId { get; set; }
            public string Culture { get; set; }
        }

        internal class CreateApplicationResult
        {
            public string culture { get; set; }
            public string userAgent { get; set; }
            public _Links _links { get; set; }
            public _Embedded _embedded { get; set; }
            public string rel { get; set; }
        }

        internal class _Links
        {
            public Link self { get; set; }
            public Link policies { get; set; }
            public Link batch { get; set; }
            public Link events { get; set; }
        }

        internal class _Embedded
        {
            public Me me { get; set; }
            public People people { get; set; }
            public Onlinemeetings onlineMeetings { get; set; }
            public Communication communication { get; set; }
        }

        internal class Me
        {
            public string name { get; set; }
            public string uri { get; set; }
            public string[] emailAddresses { get; set; }
            public string title { get; set; }
            public string department { get; set; }
            public _Links1 _links { get; set; }
            public string rel { get; set; }
        }

        internal class _Links1
        {
            public Link self { get; set; }
            public Link makeMeAvailable { get; set; }
            public Link callForwardingSettings { get; set; }
            public Link phones { get; set; }
            public Link photo { get; set; }
        }

        internal class People
        {
            public _Links2 _links { get; set; }
            public string rel { get; set; }
        }

        internal class _Links2
        {
            public Link self { get; set; }
            public Link presenceSubscriptions { get; set; }
            public Link subscribedContacts { get; set; }
            public Link presenceSubscriptionMemberships { get; set; }
            public Link myGroups { get; set; }
            public Link myGroupMemberships { get; set; }
            public Link myContacts { get; set; }
            public Link myPrivacyRelationships { get; set; }
            public Link myContactsAndGroupsSubscription { get; set; }
            public Link search { get; set; }
        }

        internal class Onlinemeetings
        {
            public _Links3 _links { get; set; }
            public string rel { get; set; }
        }

        internal class _Links3
        {
            public Link self { get; set; }
            public Link myOnlineMeetings { get; set; }
            public Link onlineMeetingDefaultValues { get; set; }
            public Link onlineMeetingEligibleValues { get; set; }
            public Link onlineMeetingInvitationCustomization { get; set; }
            public Link onlineMeetingPolicies { get; set; }
            public Link phoneDialInInformation { get; set; }
            public Link myAssignedOnlineMeeting { get; set; }
        }

        internal class Communication
        {
            public string b03eb5dc4b544422ad1c7879c035a091 { get; set; }
            public object[] supportedModalities { get; set; }
            public string[] supportedMessageFormats { get; set; }
            public _Links4 _links { get; set; }
            public string rel { get; set; }
            public string etag { get; set; }
        }

        internal class _Links4
        {
            public Link self { get; set; }
            public Link startPhoneAudio { get; set; }
            public Link conversations { get; set; }
            public Link startMessaging { get; set; }
            public Link startOnlineMeeting { get; set; }
            public Link joinOnlineMeeting { get; set; }
        }

        internal class Availability
        {
            public Availability(string status)
            {
                availability = status;
            }

            public string availability { get; set; }
        }

        internal class MakeMeAvailableContent
        {
            public MakeMeAvailableContent(string phoneNumber,
                                          string signInAs)
            {
                PhoneNumber = phoneNumber;
                SignInAs = signInAs;
            }

            public string PhoneNumber { get; set; }
            public string SignInAs { get; set; }
            public string[] SupportedMessageFormats { get; set; }
            public string[] SupportedModalities { get; set; }
        }


        internal class AvailabilityResult
        {
            public string availability { get; set; }
            public _Links5 _links { get; set; }
            public string rel { get; set; }
        }

        internal class _Links5
        {
            public Link self { get; set; }
        }

    }
}
