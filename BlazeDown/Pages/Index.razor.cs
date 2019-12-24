using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazeDown.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; }
        protected string FileUrl { get; set; }
        protected string ContentValue { get; set; }

        protected void TextChanged(ChangeEventArgs e)
        {
            ContentValue = e.Value.ToString();
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender == true)
            {
                ContentValue = await GetContentFromUrl("/sample-data/example.md");
                StateHasChanged();
            }
        }

        protected async void OnImportClicked(EventArgs e)
        {
            string path = String.IsNullOrWhiteSpace(FileUrl) ? "/sample-data/example.md" : FileUrl;
            ContentValue = await GetContentFromUrl(path);
            StateHasChanged();
        }

        private async Task<string> GetContentFromUrl(string path)
        {
            HttpResponseMessage httpResponse = await Http.GetAsync(path);
            return httpResponse.IsSuccessStatusCode ?
            await httpResponse.Content.ReadAsStringAsync() : httpResponse.ReasonPhrase;
        }
    }
}
