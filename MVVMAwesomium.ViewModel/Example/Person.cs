using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVVMAwesonium.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MVVMAwesomium.ViewModel.Infra;

namespace MVVMAwesonium.ViewModel.Example
{
    public class Person : ViewModelBase
    {
        public Person()
        {
            Skills = new ObservableCollection<Skill>();
            Command = new RelayCommand(DoCommand);
            RemoveSkill = new RelayCommand<Skill>(s=> this.Skills.Remove(s));
            ChangeSkill = new RelayCommand<Skill>(s => MainSkill = (this.Skills.Count>0)?this.Skills[0] : null);
        }




        private void DoCommand()
        {
            Local = new Local() { City = "Paris", Region = "IDF" };
            Skills.Insert(0, new Skill() { Name = "Info", Type = "javascript" });

            //_FirstSkill.Name = "Lingua";
            //_FirstSkill.Type = "Frances";
            //_Person.Local.City = "ded";
            //_Person.Skills.RemoveAt(_Person.Skills.Count-1);
            //_Person.Skills[0]= new Skill() { Name = "Info", Type = "knockout" };
        }

        private string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set
            {
                Set(ref _LastName, value, "LastName");
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                Set(ref _Name, value, "Name");
            }
        }

        private int _Age;
        public int Age
        {
            get { return _Age; }
            set
            {
                Set(ref _Age, value, "Age");
            }
        }

        private Local _Local;
        public Local Local
        {
            get { return _Local; }
            set
            {
                Set(ref _Local, value, "Local");
            }
        }

        private Skill _MainSkill;
        public Skill MainSkill
        {
            get { return _MainSkill; }
            set
            {
                Set(ref _MainSkill, value, "MainSkill");
            }
        }

        public IList<Skill> Skills { get; private set; }

        public ICommand Command { get; private set; }

        public ICommand RemoveSkill { get; private set; }

        public ICommand ChangeSkill { get; private set; }
    }
}
