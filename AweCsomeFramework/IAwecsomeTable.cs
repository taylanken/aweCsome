﻿using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweCsome
{
    public interface IAweCsomeTable
    {
        void CreateTable<T>();
        void DeleteTable<T>();
        void DeleteTableIfExisting<T>();
        int InsertItem<T>(T entity);
        T SelectItemById<T>(int id) where T : new();
        List<T> SelectAllItems<T>() where T : new();
        List<T> SelectItemsByFieldValue<T>(string fieldname, object value) where T : new();
        List<T> SelectItemsByQuery<T>(string query) where T : new();
        void UpdateItem<T>(T entity);
        void DeleteItemById<T>(int id);
        string[] GetAvailableChoicesFromField<T>(string propertyname);
    }
}