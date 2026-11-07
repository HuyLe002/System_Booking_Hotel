import { RouterProvider } from "react-router";
import { AuthProvider } from "../auth/AuthProvider";
import { ToastProvider } from "../components/ui";
import { router } from "./routes";

export default function App() {
  return (
    <ToastProvider>
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </ToastProvider>
  );
}
