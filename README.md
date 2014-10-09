MVVM for awesomium
==================

Description
--------------
This framework act as a glue between a C# ViewModel and a awesomium IWebView.
This allows to reuse 100% of View Model written for WPF in a HTML UI(provided you implement correctly the INotifyPropertyChanged, INotifyCollectionChanged and ICommand patterns).  
On the javascript side, C# objects are converted to javascript objects and can be bound using knockoutjs fantastic library.  
All changes between View and ViewModel are propagate back and forth by MVVM for awesomium framework using knockoutjs subscribers and C# events.
C# commands are mapped to javascript method and can be bind to click event using knockout.  
This library can be used without having to write any javascript on your own as it will take care of all the mapping and plumbing for you!


Usage - Example
--------------

**ViewModel (C#)**

		public class ViewModelBase : INotifyPropertyChanged
		{
			protected void Set<T>(ref T ipnv, T value, string ipn)
			{
				if (object.Equals(ipnv, value))
					return;
				ipnv = value;
				OnPropertyChanged(ipn);
			}

			private void OnPropertyChanged(string pn)
			{
				if (PropertyChanged == null)
					return;

				PropertyChanged(this, new PropertyChangedEventArgs(pn));
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}

		public class Skill : ViewModelBase
		{
			private string _Type;
			public string Type
			{
				get { return _Type; }
				set { Set(ref _Type, value, "Type"); }
			}

			private string _Name;
			public string Name
			{
				get { return _Name; }
				set { Set(ref _Name, value, "Name"); }
			}
		}

		public class Person: ViewModelBase
		{
			public Person()
			{
				Skills = new ObservableCollection<Skill>();
				RemoveSkill = new RelayCommand<Skill>(s=> this.Skills.Remove(s));
			}
		  
			private string _LastName;
			public string LastName
			{
				get { return _LastName; }
				set { Set(ref _LastName, value, "LastName"); }
			}

			private string _Name;
			public string Name
			{
				get { return _Name; }
				set { Set(ref _Name, value, "Name"); }
			}
		   
			public IList<Skill> Skills { get; private set; }

			public ICommand RemoveSkill { get; private set; }
		}
		
		
**View (HTML) using knockout mark-up**

	<!doctype html>
	<html>
		<head>
			<title></title>
			<script src="js/knockout.js" type="text/javascript"></script>
			<script src="js/Ko_Extension.js" type="text/javascript"></script>
		</head>
		<body>
			<input type="text" data-bind="value: Name, valueUpdate:'afterkeydown'" placeholder="First name" >
			<ul data-bind="foreach: Skills">
				<li><span data-bind="text:Type"></span>:<span data-bind="text:Name"></span>
				<button data-bind="command: $root.RemoveSkill">Remove skill</button></li>
			</ul>
			<div>
				<h2><span data-bind="text: Name"></span></h2>
				<h2><span data-bind="text: LastName"></span></h2>
			</div>

			<button data-bind="command: ChangeSkill">Click me</button>
		</body>
	</html>

	
**Set the binding (C#)**

	IWebView mywebview = ...; //retrieve awesomium view wich loaded the HTML view
	var datacontext = new Person();
	AwesomeBinding.Bind(mywebview, datacontext, JavascriptBindingMode.TwoWay);


That's it!

[Documentation to nuget here.](https://github.com/David-Desmaisons/MVVM-for-awesomium/wiki/)

[Link to nuget here.](https://www.nuget.org/packages/MVVM.awesomium/)
