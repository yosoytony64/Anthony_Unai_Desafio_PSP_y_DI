using System;
using System.Collections.Generic;
using System.Text;
using Sistema_Gestion.Helpers;

namespace Sistema_Gestion.ViewModels
{

    // Clase base de todos los ViewModels. Hereda de ObservableObject,
    // por lo que ya trae INotifyPropertyChanged.

    public abstract class ViewModelBase : ObservableObject
    {
        private string _mensajeEstado = "";

        // Texto para mostrar al usuario (éxito, errores de validación...).
        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }
    }
}