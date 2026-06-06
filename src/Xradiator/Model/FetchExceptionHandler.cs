using System;
using Xradiator.Views;
using log4net;

namespace Xradiator.Model
{
	public class FetchExceptionHandler
	{
		static readonly ILog _log = LogManager.GetLogger(nameof(FetchExceptionHandler));

		readonly IXradiatorView _view;

		public FetchExceptionHandler(IXradiatorView view)
		{
			_view = view;
		}

		public void Handle(Exception fetchException)
		{
			_log.Error(fetchException.Message, fetchException);
			_view.DataContext = null;

			try
			{
				_view.Invoke(() => _view.ShowMessage("Connection problem"));
			}
			catch (Exception exception)
			{	// this really shouldn't happen but we're getting an inexplicable Win32Exception
				_log.Error(exception.Message, exception);
			}
		}
	}
}