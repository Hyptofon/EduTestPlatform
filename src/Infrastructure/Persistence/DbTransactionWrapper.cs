using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence;

public class DbTransactionWrapper(IDbContextTransaction transaction) : IDbTransaction
{
    // Виправлено: беремо стан з Connection, а не з Transaction, і додаємо перевірку на null
    public ConnectionState ConnectionState => transaction.GetDbTransaction().Connection?.State ?? ConnectionState.Closed;
    
    public IDbConnection? Connection => transaction.GetDbTransaction().Connection;
    
    public IsolationLevel IsolationLevel => transaction.GetDbTransaction().IsolationLevel;

    public void Commit() => transaction.Commit();

    public void Rollback() => transaction.Rollback();

    public void Dispose() => transaction.Dispose();
}