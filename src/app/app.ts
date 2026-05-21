import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `
    <header class="app-header">
      <h1 class="app-title">PCRC</h1>
    </header>
    <main>
      <router-outlet />
    </main>
  `,
  styles: [`
    .app-header {
      background: #1976d2;
      color: white;
      padding: 12px 24px;
      display: flex;
      align-items: center;
    }
    .app-title {
      margin: 0;
      font-size: 1.25rem;
      font-weight: 600;
    }
    main {
      max-width: 1400px;
      margin: 0 auto;
    }
  `],
})
export class App {}
