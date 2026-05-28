Read these files carefully:
- distribution-list.component.ts
- distribution-form.component.ts
- distribution-form.component.html
- InventoryAPI/Controllers/DistributionController.cs

Apply these fixes:

FIX 1 — buildCache missing fullName:
In distribution-list.component.ts,
find buildCache() method.
Change:
cache[id] = u?.name ?? u?.Name 
         ?? u?.email ?? `#${id}`;
To:
cache[id] = u?.fullName ?? u?.FullName 
         ?? u?.name ?? u?.Name 
         ?? u?.email ?? `#${id}`;

FIX 2 — Silent error on "Toutes" tab:
In distribution-list.component.ts,
add property: errorAll = '';

In loadAll() error handler:
Change:
error: () => { 
  this.loadingAll = false; 
}
To:
error: () => {
  this.errorAll = 
    'Impossible de charger les distributions. ' +
    'Vérifiez vos droits d\'accès.';
  this.loadingAll = false;
  this.cdr.markForCheck();
}

In loadAll() reset:
if (reset) { 
  this.allDistributions = []; 
  this.allPage = 1; 
  this.hasMore = true;
  this.errorAll = '';  // ← add this
}

In distribution-list.component.html,
after the loading spinner in "all" tab, add:
@if (errorAll) {
  <div class="error-banner">{{ errorAll }}</div>
}

FIX 3 — Use reactive disable instead of attr.disabled:
In distribution-form.component.ts,
in ngOnInit() after form init, add:
this.form.get('id_Stock')?.disable();

In valueChanges of id_Delegue:
if (id) {
  this.loadStocks(+id);
} else {
  this.form.get('id_Stock')?.disable();
  this.stocks = [];
}

In loadStocks() on success:
next: s => {
  this.stocks = s;
  this.loadingStocks = false;
  if (s.length > 0) {
    this.form.get('id_Stock')?.enable();
  }
  this.cdr.markForCheck();
}

In distribution-form.component.html,
remove [attr.disabled] from id_Stock select:
Change:
<select formControlName="id_Stock" 
        [attr.disabled]="!f['id_Delegue'].value 
                         ? true : null">
To:
<select formControlName="id_Stock">



Republish InventoryAPI after Fix 3.
Do not modify ocelot.json.
Do not modify MAUI files.