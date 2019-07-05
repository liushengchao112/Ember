using System.Collections;

namespace UI
{
    public class MainLeftBarController : ControllerBase
    {
        private MainLeftBarView view;

        public MainLeftBarController( MainLeftBarView v )
        {
            view = v;
            viewBase = v;
        }
    }
}

