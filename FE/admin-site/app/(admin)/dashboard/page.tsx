export default function DashboardPage() {
  return (
    <div className="space-y-6 p-2">
      <h1 className="text-3xl font-bold">Dashboard</h1>
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-lg border bg-card p-6 shadow-sm">
          <h3 className="text-sm font-medium text-muted-foreground">Total Products</h3>
          <p className="text-2xl font-bold">0</p>
        </div>
        <div className="rounded-lg border bg-card p-6 shadow-sm">
          <h3 className="text-sm font-medium text-muted-foreground">Total Orders</h3>
          <p className="text-2xl font-bold">0</p>
        </div>
        <div className="rounded-lg border bg-card p-6 shadow-sm">
          <h3 className="text-sm font-medium text-muted-foreground">Customers</h3>
          <p className="text-2xl font-bold">0</p>
        </div>
        <div className="rounded-lg border bg-card p-6 shadow-sm">
          <h3 className="text-sm font-medium text-muted-foreground">Revenue</h3>
          <p className="text-2xl font-bold">$0</p>
        </div>
      </div>
    </div>
  );
}