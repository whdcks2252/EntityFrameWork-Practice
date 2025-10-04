using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;

namespace WpfApp1.Entity
{
    public partial class EntityFrameworkVM : ObservableObject
    {
        public ObservableCollection<Person> People { get; } = new();

        [ObservableProperty] private Person? selectedPerson;

        public EntityFrameworkVM()
        {
            _ = LoadAsync();

        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            People.Clear();
            using var db = new AppDbContext();
            var list = await db.People.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
            foreach (var p in list) People.Add(p);
        }

        [RelayCommand]
        private void Add()
        {
            People.Add(new Person { Name = "New", Age = 20, IsActive = true });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            using var db = new AppDbContext();

            // 새로 추가된 항목( Id == 0 )은 Add, 기존은 Update
            foreach (var p in People)
            {
                if (p.Id == 0) db.Add(p);
                else db.Update(p);     // 비추적 상태라도 상태를 Modified로 붙여 저장
            }

            await db.SaveChangesAsync();

            // 저장 후 목록 재조회(키/값 최신화)
            await LoadAsync();
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (SelectedPerson is null) return;
            using var db = new AppDbContext();
            // 키만으로 프록시 엔티티 생성 → 삭제
            db.Entry(new Person { Id = SelectedPerson.Id }).State = EntityState.Deleted;
            await db.SaveChangesAsync();

            People.Remove(SelectedPerson);
            SelectedPerson = null;
        }

    }

    public class AppDbContext : DbContext
    {
        public DbSet<Person> People => Set<Person>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=C:/Users/whdck/Documents/github/EntityFrameWork-Practice/src/WpfApp1/bin/Debug/net8.0-windows/DB/TEST.db"); // 로컬 파일

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Person>().HasKey(x => x.Id);
            b.Entity<Person>().Property(x => x.Name).HasMaxLength(100).IsRequired();
        }
    }

    public partial class Person : ObservableObject
    {
        [ObservableProperty] private int _id;         // PK
        [ObservableProperty] private string _name = "";
        [ObservableProperty] private int _age;
        [ObservableProperty] private bool _isActive = true;
    }
}
