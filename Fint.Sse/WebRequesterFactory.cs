namespace Fint.Sse
{
    class WebRequesterFactory : IWebRequesterFactory
    {
        public IWebRequester Create()
        {
            return new WebRequester();
        }
    }
}
