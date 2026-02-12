# Database Patterns

Entity Framework Core patterns for feature-based organization.

## DbContext Partial Class Pattern

**Feature-based DbContext organization** using partial classes.

### Structure

```
src/
├── Database/
│   └── AppDbContext.cs           // Base partial class with constructor + OnModelCreatingPartial call
└── Features/
    └── Orders/
        └── Database/
            ├── AppDbContext.cs   // Partial class: DbSet + OnModelCreatingPartial override
            ├── OrderConfiguration.cs
            └── OrderItemConfiguration.cs
```

### Naming Conventions

| Element              | Convention                                                  |
| -------------------- | ----------------------------------------------------------- |
| Base DbContext       | `src/Database/{ProjectName}DbContext.cs`                    |
| Feature DbContext    | `src/Features/{Feature}/Database/{ProjectName}DbContext.cs` |
| Entity Configuration | `src/Features/{Feature}/Database/{Entity}Configuration.cs`  |

### Pattern Summary

1. Base DbContext declares `partial void OnModelCreatingPartial(ModelBuilder)` and calls it from `OnModelCreating`
2. Each feature's partial DbContext adds `DbSet<T>` properties and overrides `OnModelCreatingPartial`
3. Entity configurations implement `IEntityTypeConfiguration<TEntity>`
4. Register configurations with `modelBuilder.ApplyConfiguration(new EntityConfiguration())`

## References

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Entity Configuration](https://learn.microsoft.com/en-us/ef/core/modeling/)
